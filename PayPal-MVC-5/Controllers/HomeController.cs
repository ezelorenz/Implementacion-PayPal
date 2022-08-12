using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using PayPal_MVC_5.Models.PayPal_Order;
using PayPal_MVC_5.Models.PayPal_Transaction;

namespace PayPal_MVC_5.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {

            
            string token = Request.QueryString["token"];

            bool status = false;


            using (var client = new HttpClient())
            {


                var userName = "ARug5-LbmTDkUugmhN1SQPa0tyqATBVkxpGjmWkzGWoFwLVqMbc74KM_15a9Rc67JKYof6WBHOaInRfL";
                var password = "EBihOuoTeAgTpxMKa7RXZhd_4v2AssqFAHgOqv4oTvXF4ExpqByLyH4icQ1CdiY7UbFx_3qepqARPqch";

                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");

                var authToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

                var data = new StringContent("{}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"/v2/checkout/orders/{token}/capture", data);


                status = response.IsSuccessStatusCode;

                ViewData["Status"] = status;
                if (status)
                {
                    var jsonRespuesta = response.Content.ReadAsStringAsync().Result;

                    PayPalTransaction objeto = JsonConvert.DeserializeObject<PayPalTransaction>(jsonRespuesta);

                    ViewData["IdTransaccion"] = objeto.purchase_units[0].payments.captures[0].id;
                }

            }


            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // API solicitud de compra
        [HttpPost]       
        public async Task<JsonResult> Paypal(string precio, string producto)
        {



            bool status = false;
            string respuesta = string.Empty;

            using (var client = new HttpClient())
            {
                var userName = "ARug5-LbmTDkUugmhN1SQPa0tyqATBVkxpGjmWkzGWoFwLVqMbc74KM_15a9Rc67JKYof6WBHOaInRfL";
                var password = "EBihOuoTeAgTpxMKa7RXZhd_4v2AssqFAHgOqv4oTvXF4ExpqByLyH4icQ1CdiY7UbFx_3qepqARPqch";

                client.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");

                //codificar a byte el usuario y la contraseña, para despues pasarla a string
                var authToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));


                var orden = new PayPalOrder()
                {
                    intent = "CAPTURE",
                    purchase_units = new List<Models.PayPal_Order.PurchaseUnit>() {

                        new Models.PayPal_Order.PurchaseUnit() {

                            amount = new Models.PayPal_Order.Amount() {
                                currency_code = "USD",
                                value = precio
                            },
                            description = producto
                        }
                    },
                    application_context = new ApplicationContext()
                    {
                        brand_name = "Mi Tienda",
                        landing_page = "NO_PREFERENCE",
                        user_action = "PAY_NOW", //para mostrar el monto de pago
                        return_url = "https://localhost:44346/Home/About",
                        cancel_url = "https://localhost:44346/Home/Index"
                    }
                };


                var json = JsonConvert.SerializeObject(orden);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("/v2/checkout/orders", data);


                status = response.IsSuccessStatusCode;


                if (status)
                {
                    respuesta = response.Content.ReadAsStringAsync().Result;
                }



            }

            return Json(new { status = status, respuesta = respuesta }, JsonRequestBehavior.AllowGet);

        }
        

    }
}