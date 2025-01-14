namespace Matomo.Maui.Services.Shell
{
    /// <summary>
    /// Utility class for use with Shell Navigation
    /// </summary>
	public class ShellHelper : IShellHelper
	{
        /// <summary>
        /// Get the current Shell Navigation Path
        /// </summary>
        public string CurrentPath
        {
            get
            {
                var path = "";
                
                foreach (var page in Microsoft.Maui.Controls.Shell.Current.Navigation.NavigationStack)
                {
                    path += GetPageName($"{page}") + "/";
                }
                if (path.Equals("/"))
                    path = GetPageName($"{Microsoft.Maui.Controls.Shell.Current.CurrentPage}") + "/";
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

