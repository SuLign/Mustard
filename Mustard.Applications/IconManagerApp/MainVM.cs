using Mustard.UI.MVVM;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IconManagerApp;

internal class MainVM : ViewModelBase
{
    private ObservableCollection<PackIcon> searchResultIcons;
    private ObservableCollection<PackIcon> allIcons;
    private string searchPattern;
    private List<string> allIconsKinds;
    private PackIcon selectedIcon;

    public ObservableCollection<PackIcon> AllIcons
    {
        get => allIcons;
        set
        {
            allIcons = value;
            Set();
        }
    }

    public ObservableCollection<PackIcon> SearchResultIcons
    {
        get => searchResultIcons;
        set
        {
            searchResultIcons = value;
            Set();
        }
    }

    public PackIcon SelectedIcon
    {
        get => selectedIcon;
        set
        {
            selectedIcon = value;
            Set();
            PathCode = "";
        }
    }

    public string SearchPattern
    {
        get => searchPattern;
        set
        {
            searchPattern = value;
            Set();
            DoSearch();
        }
    }

    public string PathCode
    {
        get
        {
            return SelectedIcon == null ? "" :
                $"<Path Fill=\"Red\" Stretch=\"Uniform\" Data =\"{SelectedIcon?.Data}\"/>";
        }
        set
        {
            Set();
        }
    }

    public LazyCommand CopyData => new LazyCommand(() =>
    {
        if (SelectedIcon == null) return;
        Clipboard.SetText(SelectedIcon.Data);
    });

    public LazyCommand CopyPath => new LazyCommand(() =>
     {
         if (SelectedIcon == null) return;
         Clipboard.SetText(PathCode);
     });

    public MainVM()
    {
        var all = PackIconDataFactory.Create();
        AllIcons = new ObservableCollection<PackIcon>();
        SearchResultIcons = new ObservableCollection<PackIcon>(all);
        SelectedIcon = SearchResultIcons.ElementAt(0);
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        {
            return;
        }
        //Task.Run(() =>
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            //AllIcons = new ObservableCollection<PackIcon>(all);
            var loops = all.Count / 1000;
            var loopLeft = all.Count % 1000;
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < 1000; j++)
                {
                    AllIcons.Add(all[i * 1000 + j]);
                }
            }
            for (int j = 0; j < loopLeft; j++)
            {
                AllIcons.Add(all[loops * 1000 + j]);
            }
        });
    }

    private void DoSearch()
    {
        if (string.IsNullOrEmpty(SearchPattern))
        {
            SearchResultIcons = new ObservableCollection<PackIcon>(AllIcons);
            return;
        }
        var pattern = SearchPattern.ToLower();
        if (allIconsKinds == null)
        {
            allIconsKinds = AllIcons.Select(e => e.PackIconKind.ToString().ToLower()).ToList();
        }
        var items = from item in AllIcons where item.PackIconKind.ToString().ToLower().Contains(pattern) select item;
        SearchResultIcons = new ObservableCollection<PackIcon>(items);
        if(items != null && items.Count() > 0) SelectedIcon = items.ElementAt(0);
    }
}
