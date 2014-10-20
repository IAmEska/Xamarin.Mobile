using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Provider;

using Uri = Android.Net.Uri;

namespace Xamarin.Contacts
{
    internal static class ContactGroupHelper
    {
        internal static IEnumerable<ContactGroup> GetContactGroups(bool enableSummary, ContentResolver content, Resources resources)
        {
            Uri curi = enableSummary ? ContactsContract.Groups.ContentSummaryUri : ContactsContract.Groups.ContentUri;
            ICursor cursor = null;
            try
            {
                cursor = content.Query(curi, null, null, null, null);
                if (cursor == null)
                    yield break;

                foreach (ContactGroup group in GetContactGroups(enableSummary, content, resources, cursor, 20))
                    yield return group;
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
        }

        internal static IEnumerable<ContactGroup> GetContactGroups(bool enableSummary, ContentResolver content, Resources resources, ICursor cursor, int batchSize)
        {
            if (cursor == null)
                yield break;

            string column = Android.Provider.BaseColumns.Id;

            string[] ids = new string[batchSize];
            int columnIndex = cursor.GetColumnIndex(column);

            HashSet<string> uniques = new HashSet<string>();

            int i = 0;
            while (cursor.MoveToNext())
            {
                if (i == batchSize)
                {
                    i = 0;
                    foreach (ContactGroup g in GetContactGroups(enableSummary, content, resources, ids))
                        yield return g;
                }

                string id = cursor.GetString(columnIndex);
                if (uniques.Contains(id))
                    continue;

                uniques.Add(id);
                ids[i++] = id;
            }

            if (i > 0)
            {
                foreach (ContactGroup g in GetContactGroups(enableSummary, content, resources, ids.Take(i).ToArray()))
                    yield return g;
            }
        }

        internal static IEnumerable<ContactGroup> GetContactGroups(bool enableSummary, ContentResolver content, Resources resources, string[] ids)
        {
            Uri curi = enableSummary ? ContactsContract.Groups.ContentSummaryUri : ContactsContract.Groups.ContentUri;
            ICursor c = null;
            string column = Android.Provider.BaseColumns.Id;
            StringBuilder whereb = new StringBuilder();
            for (int i = 0; i < ids.Length; i++)
            {
                if (i > 0)
                    whereb.Append(" OR ");

                whereb.Append(column);
                whereb.Append("=?");
            }

            int x = 0;
            var map = new Dictionary<string, ContactGroup>(ids.Length);
            try
            {
                ContactGroup currentGroup = null;
                c = content.Query(curi, null, whereb.ToString(), ids, column);
                if (c == null)
                    yield break;

                int idIndex = c.GetColumnIndex(column);
                while (c.MoveToNext())
                {
                    string id = c.GetString(idIndex);
                    if (currentGroup == null || currentGroup.Id != id)
                    {
                        if (currentGroup != null)
                        {
                            if (currentGroup.Id == id)
                            {
                                yield return currentGroup;
                                x++;
                            }
                            else
                                map.Add(currentGroup.Id, currentGroup);

                        }

                        currentGroup = new ContactGroup();
                        currentGroup.Id = id;
                    }
                    FillContactGroup(c, currentGroup);
                }

                if (currentGroup != null)
                    map.Add(currentGroup.Id, currentGroup);

                for (; x < ids.Length; x++)
                {
                    if (map.ContainsKey(ids[x]))
                        yield return map[ids[x]];
                }
                    
            }
            finally
            {
                if (c != null)
                    c.Close();
            }
        }

        internal static void FillContactGroup(ICursor cursor, ContactGroup group)
        {
            group.Deleted = cursor.GetInt(cursor.GetColumnIndex(ContactsContract.GroupsColumns.Deleted));
            group.GroupVisible = cursor.GetInt(cursor.GetColumnIndex(ContactsContract.GroupsColumns.GroupVisible));
            group.Id = cursor.GetString(Android.Provider.BaseColumns.Id);
            group.Notes = cursor.GetString(ContactsContract.GroupsColumns.Notes);
            group.ShoudSync = cursor.GetInt(cursor.GetColumnIndex(ContactsContract.GroupsColumns.ShouldSync));

            //FIXME data_set is not const value for ContactContract.GroupsColumns.DataSet
            group.DataSet = cursor.GetString(cursor.GetColumnIndex("data_set"));

            int sumIndex = cursor.GetColumnIndex(ContactsContract.GroupsColumns.SummaryCount);
            if (sumIndex > -1)
                group.SummaryCount = cursor.GetInt(sumIndex);

            int sumPhoneIndex = cursor.GetColumnIndex(ContactsContract.GroupsColumns.SummaryWithPhones);
            if (sumPhoneIndex > -1)
                group.SummaryWithPhones = cursor.GetInt(sumPhoneIndex);
            
            group.SystemId = cursor.GetString(ContactsContract.GroupsColumns.SystemId);
            group.Title = cursor.GetString(ContactsContract.GroupsColumns.Title);
        }

        internal static ContactGroup GetContactGroup(ICursor cursor)
        {
            ContactGroup group = new ContactGroup();
            FillContactGroup(cursor, group);
            return  group;
        }
    }
}

