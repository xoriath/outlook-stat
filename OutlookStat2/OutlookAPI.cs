using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace OutlookTest
{
    class OutlookAPI
    {
        public struct Item 
        {
            private Outlook.Folder folder;
            private Outlook.MailItem mail;
            public DateTime Received => mail.ReceivedTime;
            public String Subject => mail.Subject;
            public String Folder => folder.Name;
            

            public Item(Outlook.Folder folder, Outlook.MailItem mail) 
            {
                this.folder = folder;
                this.mail = mail;
            }

            public override string ToString() => $"[{Folder}] [{Received}] {Subject}";
        }

        public static IEnumerable<IGrouping<string, Item>> FetchUnreads()
        {
            var outlook = new Outlook.Application();
            var mapi = outlook.GetNamespace("MAPI");
            var inbox = (Outlook.Folder)mapi.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            var explorer = inbox.GetExplorer(false);

            return EnumerateUnreads(inbox).OrderBy(i => i.Received).GroupBy(i => i.Folder);
            
        }

        private static IEnumerable<Item> EnumerateUnreads(Outlook.Folder root)
        {
            foreach(Outlook.Folder folder in root.Folders)
                foreach (Item item in EnumerateUnreads(folder))
                    yield return item;

            Outlook.Items items = root.Items.Restrict("[Unread] = true");
            foreach (object item in items) 
                if (item is Outlook.MailItem)
                    yield return new Item(root, (Outlook.MailItem)item);
        }
    }
}
