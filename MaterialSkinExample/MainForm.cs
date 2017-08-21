using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace MaterialSkinExample
{
    public partial class MainForm : MaterialForm
    {
        private readonly MaterialSkinManager _materialSkinManager;
        public MainForm()
        {
            InitializeComponent();

            // Initialize MaterialSkinManager
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);
			_materialSkinManager.Theme = MaterialSkinManager.Themes.Light;
			_materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.White);

			// Add dummy data to the listview
	        SeedListView();
        }

	    private void SeedListView()
	    {
			//Define
			var data = new[]
	        {
		        new []{"Lollipop", "392", "0.2", "0"},
				new []{"KitKat", "518", "26.0", "7"},
				new []{"Ice cream sandwich", "237", "9.0", "4.3"},
				new []{"Jelly Bean", "375", "0.0", "0.0"},
				new []{"Honeycomb", "408", "3.2", "6.5"}
	        };

			//Add
			foreach (string[] version in data)
			{
				var item = new ListViewItem(version);
				materialListView1.Items.Add(item);
			}
	    }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            _materialSkinManager.Theme = _materialSkinManager.Theme == MaterialSkinManager.Themes.Dark ? MaterialSkinManager.Themes.Light : MaterialSkinManager.Themes.Dark;
        }

	    private int _colorSchemeIndex;
        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
	        _colorSchemeIndex++;
	        if (_colorSchemeIndex > 3) _colorSchemeIndex = 0;

			//These are just example color schemes
	        switch (_colorSchemeIndex)
	        {
				case 0:
					_materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.White);
			        break;
				case 1:
					_materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Pink200, TextShade.White);
			        break;
				case 2:
					_materialSkinManager.ColorScheme = new ColorScheme(Primary.Green600, Primary.Green700, Primary.Green200, Accent.Red100, TextShade.White);
					break;
                case 3:
                    _materialSkinManager.ColorScheme = new ColorScheme(Primary.EasternBlue700, Primary.EasternBlue500, Primary.EasternBlue500, Accent.Green400, TextShade.White);
                    break;
            }
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            materialProgressBar1.Value = Math.Min(materialProgressBar1.Value + 10, 100);
        }

        private void materialFlatButton4_Click(object sender, EventArgs e)
        {
            materialProgressBar1.Value = Math.Max(materialProgressBar1.Value - 10, 0);
        }
    }
}
