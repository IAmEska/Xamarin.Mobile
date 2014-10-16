using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Provider;

namespace Xamarin.Contacts
{
    public class ContactGroupReader 
        : IEnumerable<ContactGroup>
    {
        internal ContactGroupReader(bool enableSummary, ContentQueryTranslator translator, ContentResolver content, Resources resources)
        {
            this.translator = translator;
            this.content = content;
            this.resources = resources;
            this.enableSummary = enableSummary;
        }

        public IEnumerator<ContactGroup> GetEnumerator()
        {
            Android.Net.Uri table = enableSummary ? ContactsContract.Groups.ContentSummaryUri : ContactsContract.Groups.ContentUri;

            string query = null;
            string[] parameters = null;
            string sortString = null;
            string[] projections = null;

            if (this.translator != null)
            {
                table = this.translator.Table;
                query = this.translator.QueryString;
                parameters = this.translator.ClauseParameters;
                sortString = this.translator.SortString;

                if (this.translator.Projections != null)
                {
                    projections = this.translator.Projections
                        .Where(p => p.Columns != null)
                        .SelectMany(t => t.Columns)
                        .ToArray();
                    if (projections.Length == 0)
                        projections = null;
                }

                if (this.translator.Skip > 0 || this.translator.Take > 0)
                {
                    StringBuilder limitb = new StringBuilder();

                    if (sortString == null)
                        limitb.Append(Android.Provider.BaseColumns.Id);

                    limitb.Append(" LIMIT ");

                    if (this.translator.Skip > 0)
                    {
                        limitb.Append(this.translator.Skip);
                        if (this.translator.Take > 0)
                            limitb.Append(",");

                        if (this.translator.Take > 0)
                            limitb.Append(this.translator.Take);

                        sortString = (sortString == null) ? limitb.ToString() : sortString + limitb;
                    }
                }
            }

            ICursor cursor = null;
            try
            {
                cursor = this.content.Query(table, projections, query, parameters, sortString);
                if (cursor == null)
                    yield break;

                foreach (ContactGroup g in ContactGroupHelper.GetContactGroups(enableSummary, content, resources, cursor, BatchSize))
                    yield return g;
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly ContentQueryTranslator translator;
        private readonly ContentResolver content;
        private readonly Resources resources;
        private readonly bool enableSummary;

        private const int BatchSize = 20;
    }
}

