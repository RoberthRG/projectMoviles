using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ProductXamarin
{
    public partial class Imagen : ContentPage
    {
        private byte[] selectedImageBytes;

        public Imagen()
        {
            InitializeComponent();
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await CheckAndRequestPermissionAsync<Permissions.StorageRead>();

                if (status == PermissionStatus.Granted)
                {
                    var fileResult = await FilePicker.PickAsync(new PickOptions
                    {
                        FileTypes = FilePickerFileType.Images,
                        PickerTitle = "Select an image"
                    });

                    if (fileResult != null)
                    {
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

        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (selectedImageBytes == null)
                {
                    await DisplayAlert("Error", "Please select an image before uploading.", "OK");
                    return;
                }

                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    var imageContent = new ByteArrayContent(selectedImageBytes);
                    content.Add(imageContent, "image", "selectedImage.jpg");

                    var response = await httpClient.PostAsync("https://rest-api.klevervillalva.repl.co/upload", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        // Aquí debes procesar la respuesta de la API, que debería contener el ID público y la URL segura de la imagen subida
                        // Puedes utilizar JsonConvert.DeserializeObject para convertir la respuesta JSON a un objeto si la API devuelve JSON
                        // Por ejemplo, puedes crear una clase ImageUploadResponse que contenga las propiedades public_id y secure_url y luego deserializar la respuesta así:
                        // var uploadResponse = JsonConvert.DeserializeObject<ImageUploadResponse>(responseContent);
                        // Luego, puedes utilizar uploadResponse.public_id y uploadResponse.secure_url según tus necesidades.

                        // Mostrar la ventana emergente "Imagen subida correctamente"
                        await DisplayAlert("Success", "Imagen subida correctamente", "OK");


                        // Regresar a la página anterior
                        await Navigation.PopAsync();
                        // Si la API devuelve la URL segura de la imagen, puedes actualizar la imagen en la página actual o donde sea necesario:
                        // selectedImageView.Source = ImageSource.FromUri(new Uri(uploadResponse.secure_url));
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to upload the image.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An error occurred while uploading the image: " + ex.Message, "OK");
            }
        }

        private async Task<PermissionStatus> CheckAndRequestPermissionAsync<TPermission>() where TPermission : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<TPermission>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<TPermission>();
            }

            return status;
        }
    }
}
