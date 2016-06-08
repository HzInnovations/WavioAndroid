

using Android.Support.V4.App;

using Wavio.Fragments;

namespace Wavio.Adapters
{
    class TabbedNotifsAdapter : FragmentPagerAdapter
    {
        /*private static readonly string[] Content = new[]
            {
                "All", "Recent", "Adorable", "Super Cute", "North America", "South America"
            };*/

        private string[] Content = new string[] {"All"};
        private string[] Ids = new string[] { "All" };

        public void SetTabs(string[] TabNames, string[] TabIds)
        {
            Content = TabNames;
            Ids = TabIds;
        }

        public TabbedNotifsAdapter(FragmentManager p0, string[] TabNames, string[] TabIds) 
                : base(p0) 
            {
                Content = TabNames;
            Ids = TabIds;
            }

            

            public override int Count
            {
                get { return Content.Length; }
            }

            public override Fragment GetItem(int index)
            {
                return new NotifsFragment(Content[index], Ids[index]);
            
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int p0) { return new Java.Lang.String(Content[p0 % Content.Length].ToUpper()); }
    }
}