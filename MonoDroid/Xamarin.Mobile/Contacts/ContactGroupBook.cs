using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Provider;

namespace Xamarin.Contacts
{
    public sealed class ContactGroupBook
        : IQueryable<ContactGroup>
    {
        #region IEnumerable implementation

        public IEnumerator<ContactGroup> GetEnumerator()
        {
            return ContactGroupHelper.GetContactGroups(this.EnableSummary, this.content, this.resources).GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IQueryable implementation

        public Type ElementType
        {
            get
            {
                return typeof(ContactGroup);
            }
        }

        public Expression Expression
        {
            get
            {
                return Expression.Constant(this);
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return this.contactGroupProvider;
            }
        }

        #endregion

        public Task<bool> RequestPermission()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    ICursor cursor = this.content.Query(EnableSummary ? ContactsContract.Groups.ContentSummaryUri : ContactsContract.Groups.ContentUri, null, null, null, null);
                    cursor.Dispose();

                    return true;
                }
                catch (Java.Lang.SecurityException)
                {
                    return false;
                }
            });
        }

        public ContactGroupBook(Context context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.content = context.ContentResolver;
            this.resources = context.Resources;
            this.contactGroupProvider = new ContactGroupQueryProvider(EnableSummary, content, resources);
        }

        public bool EnableSummary{ get; set; }

        private readonly ContactGroupQueryProvider contactGroupProvider;
        private readonly ContentResolver content;
        private readonly Resources resources;
    }
}

