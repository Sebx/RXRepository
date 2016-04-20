using App1.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BaseRepository<Download> entity;

        private Download lastEntity;

        public MainPage()
        {
            this.InitializeComponent();

            var context = RepositoryManager.GetInstance().Context;

            entity = (BaseRepository<Download>)context.Where((e) => e.GetType() == typeof(BaseRepository<Download>)).FirstOrDefault();

            entity.MemoryOnly = false;

            //Initialize();
        }

        public async Task Initialize()
        {
            var dateBefore = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                lastEntity = new Download() { Id = Guid.NewGuid().ToString() };
                await entity.Add(lastEntity);
            }

            var dateAfter = DateTime.Now;

            var elapsedTime = dateAfter - dateBefore;

            var dialog = new MessageDialog("Insert 100 record elapsed time: " + elapsedTime.Milliseconds.ToString());
            await dialog.ShowAsync();
        }

        private async void AddEntities_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            lastEntity = new Download() { Id = Guid.NewGuid().ToString() };
            await entity.Add(lastEntity);
        }

        private async void DelEntities_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await entity.Delete(lastEntity);
        }

        private async void UpdEntities_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            lastEntity.ProgresDownloaded += 1;
            await entity.Update(lastEntity);
        }

        private async void DelAllEntities_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await entity.DeleteAll();
        }

        private async void CountEntities_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Records on Repository: " + entity.GetCount().Result);
            await dialog.ShowAsync();
        }
    }
}
