using System;
namespace Matomo.Maui
{
    /// <summary>
    /// Utility class for use with Shell Navigation
    /// </summary>
	public class ShellHelper
	{
        private static object _syncRoot = new object();
        private static ShellHelper _instance;

        public static ShellHelper Instance
        {
            get
            {
                lock (_syncRoot)
                    if (_instance == null)
                        _instance = new ShellHelper();
                return _instance;
            }
        }

        /// <summary>
        /// Get the current Shell Navigation Path
        /// </summary>
        public string CurrentPath
        {
            get
            {
                var path = "";

                foreach (var page in Shell.Current.Navigation.NavigationStack)
                {
                    path += GetPageName($"{page}") + "/";
                }
                if (path.Equals("/"))
                    path = GetPageName($"{Shell.Current.CurrentPage}") + "/";
                return path;
            }
        }

        private ShellHelper()
		{
		}

        private string GetPageName(string page)
        {
            if (page.Contains('.'))
            {
                var idx = page.LastIndexOf('.');
                return page.Substring(idx + 1);
            }
            return page;
        }
    }
}

