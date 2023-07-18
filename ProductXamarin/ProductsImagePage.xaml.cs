using System.Collections.Generic;
using Xamarin.Forms;

namespace ProductXamarin
{
    public partial class ProductsImagePage : ContentPage
    {
        public List<ImageData> Images { get; set; }
        private ProductViewModel productViewModel;

        public ProductsImagePage(List<ImageData> images, ProductViewModel viewModel)
        {
            InitializeComponent();
            Images = images;
            productViewModel = viewModel;
            BindingContext = this;
        }
    }
}