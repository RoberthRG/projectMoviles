using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProductXamarin
{
    public partial class MainPage : ContentPage
    {
        private ProductViewModel productViewModel;
        private Product selectedProduct; // Variable para almacenar el producto seleccionado


        public MainPage()
        {
            InitializeComponent();
            productViewModel = new ProductViewModel();
            BindingContext = productViewModel;

            Appearing += MainPage_Appearing;

        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            // Actualizar automáticamente la lista de productos cada vez que la página aparece
            productViewModel.FetchProducts();
        }

        private async void OnAddProductClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NewProductPage(productViewModel));
        }


        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Product product)
            {
                bool isConfirmed = await DisplayAlert("Confirmación", $"¿Eliminar el producto '{product.name}'?", "Sí", "No");

                if (isConfirmed)
                {
                    await DeleteProductAsync(product._id);
                }
            }
        }

        private async Task DeleteProductAsync(string productId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://rest-api.klevervillalva.repl.co/products/{productId}";
                    HttpResponseMessage response = await client.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // La respuesta fue exitosa, puedes mostrar un mensaje o actualizar la lista de productos
                        // por ejemplo, llamando nuevamente a FetchProducts.
                        (BindingContext as ProductViewModel)?.FetchProducts();
                    }
                    else
                    {
                        // La respuesta no fue exitosa, maneja el error según el caso
                        // Por ejemplo, muestra un mensaje de error
                        await DisplayAlert("Error", "No se pudo eliminar el producto", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar el error, mostrar un mensaje de error, etc.
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Product product)
            {
                selectedProduct = product; // Almacena el producto seleccionado en la variable
                await Navigation.PushAsync(new EditProductPage(selectedProduct));
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Imagen());
        }
    }

    public class ProductViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> products;

        public ObservableCollection<Product> Products
        {
            get { return products; }
            set
            {
                products = value;
                OnPropertyChanged();
            }
        }

        public ProductViewModel()
        {
            FetchProducts();
        }

        public async void FetchProducts()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://rest-api.klevervillalva.repl.co/products";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    Products = JsonConvert.DeserializeObject<ObservableCollection<Product>>(json);
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Product
    {
        public string _id { get; set; } // Agregar la propiedad _id
        public string name { get; set; }
        public string category { get; set; }
        public double price { get; set; }
        public string description  { get; set; }
        public ImageData image { get; set; }
    }


    public class ImageInfo
    {
        public string public_id { get; set; }
        public string secure_url { get; set; }
    }
}
