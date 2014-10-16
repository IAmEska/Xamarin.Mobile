using System;
using System.Collections;
using Android.Content;
using Android.Content.Res;

namespace Xamarin.Contacts
{
    internal class ContactGroupQueryProvider : ContentQueryProvider
    {
        private readonly bool enableSummary;

        internal ContactGroupQueryProvider(bool enableSummary, ContentResolver content, Resources resources)
            : base(content, resources, new ContactTableFinder())
        {
            this.enableSummary = enableSummary;
        }

        protected override IEnumerable GetObjectReader(ContentQueryTranslator translator)
        {
            if (translator == null || translator.ReturnType == null || translator.ReturnType == typeof(Contact))
                return new ContactGroupReader(enableSummary, translator, content, resources);
            else if (translator.ReturnType == typeof(string))
                return new ProjectionReader<string>(content, translator, (cur, col) => cur.GetString(col));
            else if (translator.ReturnType == typeof(int))
                return new ProjectionReader<int>(content, translator, (cur, col) => cur.GetInt(col));

            throw new ArgumentException();
        }
    }
}

