using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace AzureAIDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            string key = "74f444b0708b49ea940365ced892eaf3";
            string endPoint = "https://msit133inchoufaceservice.cognitiveservices.azure.com";
            string imgUrl = "https://static.leiphone.com/uploads/new/images/20191020/5dac1e9e621fa.jpg?imageView2/2/w/740";
            HttpClient client = new HttpClient();
            using (HttpResponseMessage imgResponse=await client.GetAsync(imgUrl))
            {
                imgResponse.EnsureSuccessStatusCode();
                using(Stream imgStream = await imgResponse.Content.ReadAsStreamAsync())
                {
                    pictureBox1.Image = Image.FromStream(imgStream);
                }
            }
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-key", key);
            queryString["returnFaceId"] = "true";
            queryString["returnFaceLandmarks"] = "false";
            queryString["returnFaceAttributes"] = "age,gender,emotion";
            queryString["recognitionModel"] = "recognition_01";
            queryString["returnRecognitionModel"] = "false";
            queryString["detectionModel"] = "detection_01";
            string uri = $"{endPoint}/face/v1.0/detect?{queryString}";

            JObject data = new JObject { ["url"] = imgUrl };
            string json = JsonConvert.SerializeObject(data);
            StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(uri, stringContent);
            string content = await response.Content.ReadAsStringAsync();
            //MessageBox.Show(content);
            dynamic faces = JsonConvert.DeserializeObject(content);
            foreach(var item in faces)
            {
                JObject face = item as JObject;
                int age = Convert.ToInt32(face["faceAttributes"]["age"]);
                string gender = Convert.ToString(face["faceAttributes"]["gender"]);
                JObject emotion = JObject.Parse(Convert.ToString(face["faceAttributes"]["emotion"]));
                double maxEmotion = 0;
                string maxEmotionName = "";
                foreach(JProperty prop in emotion.Properties())
                {
                    double propertyValue = double.Parse(emotion[prop.Name].ToString());
                    if (propertyValue > maxEmotion)
                    {
                        maxEmotion = propertyValue;
                        maxEmotionName = prop.Name;
                    }
                }
                MessageBox.Show($"性別：{gender},年齡：{age},情緒：{maxEmotionName},信心指數：{maxEmotion*100:n2}%");
            }
        }
    }
}
