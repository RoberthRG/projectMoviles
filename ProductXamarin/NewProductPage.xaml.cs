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

namespace ProductXamarin
{
    public partial class NewProductPage : ContentPage
    {
        private byte[] selectedImageBytes; // Para almacenar la imagen seleccionada en bytes

        private const string apiUrl = "https://rest-api.klevervillalva.repl.co/server-images";
        private ImageData selectedImage;
        private ProductViewModel productViewModel;

        public NewProductPage(ProductViewModel viewModel)
        {
            InitializeComponent();
            productViewModel = viewModel;

        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await CheckAndRequestPermissionAsync<Permissions.StorageRead>();

                if (status == PermissionStatus.Granted)
                {
                    // Abrir el selector de archivos para elegir una imagen
                    var fileResult = await FilePicker.PickAsync(new PickOptions
                    {
                        FileTypes = FilePickerFileType.Images,
                        PickerTitle = "Select an image"
                    });

                    if (fileResult != null)
                    {
                        // Leer los datos de la imagen seleccionada en bytes
                        using (var stream = await fileResult.OpenReadAsync())
                        {
                            selectedImageBytes = new byte[stream.Length];
                            await stream.ReadAsync(selectedImageBytes, 0, (int)stream.Length);
                        }

                        selectedImageView.Source = ImageSource.FromStream(() => new MemoryStream(selectedImageBytes));
                        selectedImageLabel.Text = fileResult.FileName;
                    }
                }
                else
                {
                    await DisplayAlert("Permission Denied", "The StorageRead permission is required to select an image.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An error occurred while selecting the image: " + ex.Message, "OK");
            }
        }

        // Método para comprobar y solicitar permisos de manera asíncrona
        private async Task<PermissionStatus> CheckAndRequestPermissionAsync<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<TPermission>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<TPermission>();
            }

            return status;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener los datos del producto desde las entradas de texto
                string name = nameEntry.Text;
                string category = categoryEntry.Text;
                string description = descriptionEntry.Text;
                double price = 0;
                double.TryParse(priceEntry.Text, out price);

                // Comprobar que los campos requeridos no estén vacíos
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(description) || price <= 0)
                {
                    await DisplayAlert("Error", "Please fill in all the required fields.", "OK");
                    return;
                }

                // Crear un diccionario con los datos del producto
                var productData = new Dictionary<string, object>
        {
            { "name", name },
            { "category", category },
            { "description", description },
            { "price", price }
        };

                // Si se ha seleccionado una imagen, agregarla al diccionario como un archivo de imagen
                if (selectedImageBytes != null && selectedImageBytes.Length > 0)
                {
                    var imageStreamContent = new StreamContent(new MemoryStream(selectedImageBytes));
                    var imageContent = new ByteArrayContent(selectedImageBytes);
                    imageStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "image",
                        FileName = "product_image.jpg"
                    };
                    imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                    var formData = new MultipartFormDataContent
            {
                { imageStreamContent, "image", "product_image.jpg" }
            };

                    // Agregar los datos del producto al formulario
                    foreach (var keyValuePair in productData)
                    {
                        formData.Add(new StringContent(keyValuePair.Value.ToString()), keyValuePair.Key);
                    }

                    // Realizar la solicitud POST a la API
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.PostAsync("https://rest-api.klevervillalva.repl.co/products", formData);
                        var content = await response.Content.ReadAsStringAsync();

                        // Verificar si la solicitud fue exitosa
                        if (response.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Success", "Product saved successfully.", "OK");
                            // Cerrar la página actual para volver a la ventana anterior
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Error", "Failed to save the product. Error message: " + content, "OK");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Please select an image for the product.", "OK");
                }


            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An error occurred while saving the product: " + ex.Message, "OK");
            }
        }


    }

    // Define a class to represent the image data from the API response
    public class ImageData
    {
        public string secure_url { get; set; }
        public string public_id { get; set; }
    }
}