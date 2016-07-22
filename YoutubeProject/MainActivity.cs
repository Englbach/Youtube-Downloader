using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using Android.Content;
using FFImageLoading.Views;
using FFImageLoading;

namespace YoutubeProject
{
	[Activity(Label = "YoutubeProject", MainLauncher = true, Icon = "@mipmap/icon")]

	public class MainActivity : AppCompatActivity
	{
		ListView lst;
		void Lst_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var intent = new Intent(this, typeof(VideoActivity));
			var adapter = lst.Adapter as VideoAdapter;

			intent.PutExtra("id", adapter[e.Position].id.videoId);

			StartActivity(intent);

		}

		string tag = "https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=50&type=video&key=AIzaSyDTCxULvKCL8sTlpE1gsL9bJfxB9Yj1Zks&q=";
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it

			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			toolbar.Title = "Youtube";
			toolbar.InflateMenu(Resource.Menu.itemSearch);
			toolbar.MenuItemClick += (object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e) =>
			{
				
			};

			var search = toolbar.Menu.FindItem(Resource.Id.search);
			var searchView = search.ActionView.JavaCast<Android.Support.V7.Widget.SearchView>();
			lst = FindViewById<ListView>(Resource.Id.lstView);

			searchView.QueryTextSubmit += async (object sender, Android.Support.V7.Widget.SearchView.QueryTextSubmitEventArgs e) =>
			 {
				var content = await GetRequestJson(tag + searchView.Query);
				var items = JsonConvert.DeserializeObject<Youtube.RootObject>(content);

				lst.Adapter = new VideoAdapter(Application.Context, items.items);
			 };

			lst.ItemClick+= Lst_ItemClick;
		}

		public async Task<string> GetRequestJson(string link)
		{
			HttpClient client = new HttpClient();
			HttpResponseMessage respone = await client.GetAsync(link);
			return await respone.Content.ReadAsStringAsync();
		}

		public class VideoAdapter : BaseAdapter<Youtube.Item>
		{
			LayoutInflater inflater;

			public List<Youtube.Item> video { get; set; }

			public VideoAdapter(Context context, List<Youtube.Item> videos)
			{
				inflater = LayoutInflater.FromContext(context);
				video = videos;
			}

			public override Youtube.Item this[int index]
			{
				get { return video[index];}
			}

			public override int Count
			{
				get
				{
					return video.Count;
				}
			}

			public override long GetItemId(int position)
			{
				return position;
			}

			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				//throw new System.NotImplementedException();
				View view = convertView ?? inflater.Inflate(Resource.Layout.ListViewItemLayout, parent, false);

				var item = video[position];
				var viewHolder = view.Tag as ViewHolder;
				if (viewHolder == null)
				{
					viewHolder = new ViewHolder();
					viewHolder.Title = view.FindViewById<TextView>(Resource.Id.Title);
					viewHolder.ChannelTitle = view.FindViewById<TextView>(Resource.Id.ChannelTitle);
					//viewHolder.ViewCount = view.FindViewById<TextView>(Resource.Id.ViewCount);
					viewHolder.Thumbnail = view.FindViewById<ImageViewAsync>(Resource.Id.Thumbnail);
					view.Tag = viewHolder;
					                            
				}

				viewHolder.Title.Text = item.snippet.title;
				viewHolder.ChannelTitle.Text = item.snippet.channelTitle;
				if (item.snippet.thumbnails.@default.url != null)
				{
					ImageService.Instance.LoadUrl(item.snippet.thumbnails.@default.url)
					            .Retry(5,200)
					            .Into(viewHolder.Thumbnail);
				}
				else
				{
					ImageService.Instance.LoadUrl("https://3.bp.blogspot.com/-uq0bk_FR1vw/VtAJPybeGkI/AAAAAAAAAq0/MMPAzz0ABgI/s1600/no-thumbnail.png")
								.Retry(5, 200)
								.Into(viewHolder.Thumbnail);
				}

				return view;
			}


			public class ViewHolder : Java.Lang.Object
			{
				public TextView Title { get; set; }
				public TextView ChannelTitle { get; set; }
				//public TextView ViewCount { get; set; }
				public ImageViewAsync Thumbnail { get; set; }
			}
		}
	}
}


