using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProductXamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditProductPage : ContentPage
    {
        private Product productToEdit; // Propiedad para almacenar el producto que se va a editar

        private byte[] selectedImageBytes; // Para almacenar la imagen seleccionada en bytes
        private const string apiUrl = "https://rest-api.klevervillalva.repl.co/server-images";
        private ImageData selectedImage;
        private ProductViewModel productViewModel;
        public EditProductPage(Product product)
        {
            InitializeComponent();
            productToEdit = product; // Guardar el producto recibido en la propiedad
            // Carga los datos del producto en los campos de edición
            LoadProductData();
            //productViewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Cargar los datos del producto en los campos de edición
            LoadProductData();
        }

        private void LoadProductData()
        {
            // Verificar si el producto a editar no es nulo antes de cargar los datos
            if (productToEdit != null)
            {
                // Cargar los datos del producto en los campos de edición
                nameEntry.Text = productToEdit.name;
                categoryEntry.Text = productToEdit.category;
                descriptionEntry.Text = productToEdit.description;
                priceEntry.Text = productToEdit.price.ToString();

                // Cargar la imagen desde la URL en la propiedad "secure_url" de "productToEdit.image"
                if (productToEdit.image != null && !string.IsNullOrEmpty(productToEdit.image.secure_url))
                {
                    selectedImageView.Source = new UriImageSource { Uri = new Uri(productToEdit.image.secure_url) };
                }
            }
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(apiUrl);
            var images = JsonConvert.DeserializeObject<List<ImageData>>(response);

            // Show a modal to select an image
            var selectedImageUrl = await DisplayActionSheet("Select an image", "Cancel", null, images.Select(img => img.secure_url).ToArray());
            if (!string.IsNullOrEmpty(selectedImageUrl))
            {
                this.selectedImage = images.Find(img => img.secure_url == selectedImageUrl);
                selectedImageLabel.Text = selectedImageUrl; // Show the selected URL in the label
                selectedImageView.Source = ImageSource.FromUri(new Uri(selectedImageUrl)); // Show the selected image in the ImageView
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Crear un diccionario para almacenar los datos del producto actualizado
                var updatedProductData = new Dictionary<string, object>
        {
            { "name", nameEntry.Text },
            { "category", categoryEntry.Text },
            { "description", descriptionEntry.Text },
            { "price", double.Parse(priceEntry.Text) }
        };

                // Si hay una imagen seleccionada, agregar los datos de la imagen al diccionario
                if (selectedImage != null)
                {
                    var imageDictionary = new Dictionary<string, string>
            {
                { "public_id", selectedImage.public_id },
                { "secure_url", selectedImage.secure_url }
            };
                    updatedProductData.Add("image", imageDictionary);
                }

                // Convertir el diccionario a JSON
                var updatedProductJson = JsonConvert.SerializeObject(updatedProductData);

                // Crear una solicitud PUT con la URL correspondiente al producto que se está editando
                var apiUrl = $"https://rest-api.klevervillalva.repl.co/products/{productToEdit._id}";
                var httpClient = new HttpClient();
                var content = new StringContent(updatedProductJson, System.Text.Encoding.UTF8, "application/json");

                // Enviar la solicitud PUT
                var response = await httpClient.PutAsync(apiUrl, content);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Actualizar la lista de productos llamando a FetchProducts del productViewModel si lo tienes
                    (productViewModel as ProductViewModel)?.FetchProducts();
                    // O si no, puedes hacer algo más, como mostrar un mensaje de éxito
                    await DisplayAlert("Éxito", "Producto actualizado exitosamente", "OK");
                    // Cerrar la página de edición después de guardar los cambios
                    await Navigation.PopAsync();
                }
                else
                {
                    // Si la solicitud no fue exitosa, mostrar un mensaje de error
                    await DisplayAlert("Error", "No se pudo actualizar el producto", "OK");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir durante el proceso
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}