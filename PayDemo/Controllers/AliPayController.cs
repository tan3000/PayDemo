using Alipay.AopSdk.Core;
using Alipay.AopSdk.Core.Domain;
using Alipay.AopSdk.Core.Request;
using Alipay.AopSdk.Core.Response;
using Alipay.AopSdk.F2FPay.Business;
using Alipay.AopSdk.F2FPay.Domain;
using Alipay.AopSdk.F2FPay.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace PayDemo.Controllers
{
    public class AliPayController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public AliPayController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        const string URL = "https://openapi.alipay.com/gateway.do";  //沙箱环境与正式环境不同 这里要用沙箱的 支付宝地址
                                                                     //	APPID即创建应用后生成
        const string APPID = "2021001192657314";
        //开发者应用私钥，由开发者自己生成  开发者私钥到底是什么玩意  原来开发者私钥就是商户应用私钥
        const string APP_PRIVATE_KEY = "MIIEpgIBAAKCAQEA67B/suXuWJd49usxuxgWwMrdy/yoCIHyM0ab0p5OJ9dNFSTfMA0b7MXQlNg/Mcf5lsZS07u0P7VqosVFvt4hnCBf/DEWcZ2BjWDApiNAwr9uQnEX4tFXeVeJutUL++Xx0BHErcFtvpVrgvUR7Pyosqktmb5Fq2nQwy58Qjo0fwXrAiYY+OasgHtI7RbMSYvggpxPEacw7d4OAO3IXTXIO3ifQdJYz1N7VrEajAwxQ2zx+XqU7NWGdFh6zGJDHHEC6k1zxsDBjlBOkayvaUfy9xUsY5fRRPXWBKn9usvCxsgTNoEeiOMCZ5OSGEWoP0gekP0cC33skHyWLH5p+5e5jQIDAQABAoIBAQDBetrZ2beYMoCy8tCYbmX/TopVcsvecA6L2WMBu8MdIbm9qc8uexR5gsp/5iW6kBdsltJXzrdhtWQcyyU+Usv0hv9E4Gc0TcCUduCvgjEfrlatZHnfUUsl6Z6/KTX1TAb+cBetLCUTV0Yy80wL6D30rL0qgPBgjzU5hWesVmvR+U4s846nrhEuQsOdeixANe+pjaCXhXfMCSZ/qDyutzdhpeXoAH1adZgGlamOfTPxVjB+tqP5gmrASc7QALEh5dUguXNYaJ0UT9+vxJS+cnFGaPtrWkgd+CZjhPW/bcoQ6mujVW48v+H4zZ+VX4OijGQA42KRPsD75Lhnu6RgXHFhAoGBAP6h1hnsfPtiY1KfuQM+3AutQp1TE5R8qQNurmAUJtMwgYEHDBofDev08RqbIXGtMiRzgFpMYu3Jr3e8zcvjBXId/pNkEzB+2lJ5azTF2NMbT5tdXPZOpOH+Wv3CcN4gjUOwj+Z8SyoCTiHI00GwZ3fwxgM145lDS2mOu8rzfWmnAoGBAOz0nOmQN1NDE2xYuIdTsFOsvvEtb8sa561hL3mI4MqEmh9rbnmj4s18tl5AStsT/h2OA4Ep/bXtXqvqxYdQdKFTc9HzAkcI7YitdTDGMWtXU4Ji0c/r7DUaAuXpVwDTNBbVToNzCkfQbnkW6/tGlp4cnu1XBiiKL6tR36WPY4GrAoGBAIb33XasRasyAZegFf9EaHrAAdlGWdCRKz0og8FlrRawVVTBGYcXAtgZY1tI8zdAKJ3toxE6AA4uo5WCPEtgMFjG0VPq7cj81Fh3B35XiJCNn2qo2Eeuc/NeUXjEgfMbqgVBJ2VyaJ0Bz8xUmLXu/Uk0FKFLBTMN2oE/KQdwfP9tAoGBAIjv7GaGM5LopqbstuduE+6nXEmgyoSD7fOiwH7p/Y7MBw21VkjxzzaVpgbd/OSSrz6BPcE9dSPYI+gSa0kB1AUPZ1WjrGNE45EjPSCMyS6Rbu9hEIOqgf1GJPqdWOxfIjE34IHSz9QkvlM4TfQPHSgOvkHHEwYGpfnJ/Qd+0DC1AoGBAOwPC1WjBkQO52aqlhUCiQHu3/EcxW0KTwuqsFmfozNPTIaeGg51nT3LH5lcS9zyHqRdqpPgEbV4maoq7/4guDCLw8xzDODoNG3tnSiPO12AEHojKcm0LVbIKIUdG1Mp/ouAFZK/mpS2V34qJ/xHPb52FmAgAsfCy6Oav9yk78CU";
        //参数返回格式，只支持json
        const string FORMAT = "json";
        //请求和签名使用的字符编码格式，支持GBK和UTF-8
        const string CHARSET = "UTF-8";
        //支付宝公钥，由支付宝生成     到蚂蚁金服复制
        const string ALIPAY_PUBLIC_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkVkX1GcMTGOzJgi9C1g2EEfDepwRdkB7M2tLy6e2JGMITuOkFa2YnpHOt8eG0zH8tg2Tks/3JulV7JNl0nGrd7vMSRo3YXaYUYyv68lNVKr24Q1XeRIws/e8VbxvBkbeBAiz5AkTRuzM7C3SNs78x0wLkiTwcxtwNrVp+W+zk60KG7xLoz99yAoVsfA/FT3ioGYeq9qanClWAzQmbXHC55szN12sGMAjuFNMzhYgjI3JRk4uPARtkVyjCERfARhRCvaE6HjMgP5VUJsAKJGi+SY5XnlH57DGqcol1iNqKFx0y7asTA/svDrd+xnS22Bul4IY8r93pjli2VXEUf89ZwIDAQAB";

        IAlipayTradeService serviceClient = F2FBiz.CreateClientInstance(URL, APPID, APP_PRIVATE_KEY, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET);

        public ActionResult Test()
        {
            //var ii = ts.Trim('"') == "TRADE_CLOSED" ? true : false;
            //return Content(GetJson(St.tradeStatus));
            Dictionary<string, string> openWith = new Dictionary<string, string>();
            openWith.Add("tradeno", "716");
            openWith.Add("alipayTradeNo", "");
            var url= "http://localhost:1901/AliPay/OrderClose";
            return Content(PostHtml(url,openWith));
        }

        // GET: AliPay
        public ActionResult Index()
        {
            return View();
        }

        // GET: AliPay
        public ActionResult Query()
        {
            return View();
        }


        // GET: AliPay
        public ActionResult Refund()
        {
            return View();
        }

        // GET: AliPay
        public ActionResult RefundQuery()
        {
            return View();
        }

        // GET: AliPay
        public ActionResult OrderClose()
        {
            return View();
        }

        /// <summary>
        /// 生成支付二维码
        /// </summary>
        /// <param name="orderName">订单名称</param>
        /// <param name="orderAmount">订单金额</param>
        /// <param name="outTradeNo">订单号</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FaceToPay(string orderName, string orderAmount, string outTradeNo, bool pin = false)
        {   //域名

            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：
            AlipayTradePrecreateRequest request = new AlipayTradePrecreateRequest();//创建API对应的request类


            //SDK已经封装掉了公共参数，这里只需要传入业务参数

            Random ran = new Random();
            int RandKey = ran.Next(100, 999);

            AlipayTradePayModel model = new AlipayTradePayModel
            {
                OutTradeNo = outTradeNo + RandKey,
                TotalAmount = orderAmount,
                Subject = orderName,
                //StoreId = "NJ_001",
                TimeoutExpress = "90m"
            };

            request.SetBizModel(model);

            //此次只是参数展示，未进行字符串转义，实际情况下请转义
            //request.BizContent = "{" +
            //                    "    \"out_trade_no\":\"20150320010101002\"," +
            //                    "    \"total_amount\":\"88.88\"," +
            //                    "    \"subject\":\"Iphone6 16G\"," +
            //                    "    \"store_id\":\"NJ_001\"," +
            //                    "    \"timeout_express\":\"90m\"}";

            AlipayTradePrecreateResponse response = client.Execute(request);

            AlipayTradePrecreateContentBuilder builder = BuildPrecreateContent(model.Subject, model.TotalAmount, model.OutTradeNo);
            //如果需要接收扫码支付异步通知，那么请把下面两行注释代替本行。
            //推荐使用轮询撤销机制，不推荐使用异步通知,避免单边账问题发生。
            AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder);
            //string notify_url = "http://10.5.21.14/notify_url.aspx";  //商户接收异步通知的地址
            //AlipayF2FPrecreateResult precreateResult = serviceClient.tradePrecreate(builder, notify_url);

            //payResponse.QrCode即二维码对于的链接
            //将链接用二维码工具生成二维码打印出来，顾客可以用支付宝钱包扫码支付。
            var bitmap = new Bitmap(Path.Combine(_hostingEnvironment.WebRootPath, "images/error.png"));

            //调用成功，则处理业务逻辑
            if (!response.IsError)
            {
                bitmap.Dispose();
                bitmap = RenderQrCode(response.QrCode);

                //轮询订单结果
                //根据业务需要，选择是否新起线程进行轮询
                ParameterizedThreadStart parStart = new ParameterizedThreadStart(LoopQuery);
                //Thread myThread = new Thread(parStart);
                //object o = precreateResult.response.OutTradeNo;
                //myThread.Start(o);

                var res = new
                {
                    success = true,
                    out_trade_no = response.OutTradeNo,
                    tot = model.TotalAmount,
                    suj = model.Subject,
                    qr_code = response.QrCode    //发现返回的是一个网址,后续需要用这个网址字符串创建二维码
                };

                if (pin)
                {
                    return Json(res);
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, ImageFormat.Png);
                    byte[] bytes = ms.GetBuffer();
                    return File(bytes, "image/png");
                }
            }
            else
            {
                //return Content("调用失败，原因：" + response.Msg + "，" + response.SubMsg);
                var res = new
                {
                    success = false,
                };
                return Json(res);
            }

            ////以下返回结果的处理供参考。
            ////payResponse.QrCode即二维码对于的链接
            ////将链接用二维码工具生成二维码打印出来，顾客可以用支付宝钱包扫码支付
            //var bitmap = new Bitmap(Path.Combine(_hostingEnvironment.WebRootPath, "images/error.png"));
            //switch (precreateResult.Status)
            //{
            //    case ResultEnum.SUCCESS:
            //        bitmap.Dispose();
            //        bitmap = RenderQrCode(precreateResult.response.QrCode);
            //        //轮询订单结果
            //        //根据业务需要，选择是否新起线程进行轮询
            //        ParameterizedThreadStart parStart = new ParameterizedThreadStart(LoopQuery);
            //        Thread myThread = new Thread(parStart);
            //        object o = precreateResult.response.OutTradeNo;
            //        myThread.Start(o);
            //        break;
            //    case ResultEnum.FAILED:
            //        Console.WriteLine("生成二维码失败：" + precreateResult.response.Body);
            //        break;

            //    case ResultEnum.UNKNOWN:
            //        Console.WriteLine("生成二维码失败：" + (precreateResult.response == null ? "配置或网络异常，请检查后重试" : "系统异常，请更新外部订单后重新发起请求"));
            //        break;
            //}
            //MemoryStream ms = new MemoryStream();
            //bitmap.Save(ms, ImageFormat.Png);
            //byte[] bytes = ms.GetBuffer();
            //return File(bytes, "image/png");
        }

        /// <summary>
        /// 轮询
        /// </summary>
        /// <param name="o">订单号</param>
        public void LoopQuery(object o)
        {
            AlipayF2FQueryResult queryResult = new AlipayF2FQueryResult();
            int count = 100;
            int interval = 10000;
            string out_trade_no = o.ToString();

            for (int i = 1; i <= count; i++)
            {
                Thread.Sleep(interval);
                queryResult = serviceClient.tradeQuery(out_trade_no);
                if (queryResult != null)
                {
                    if (queryResult.Status == ResultEnum.SUCCESS)
                    {
                        DoSuccessProcess(queryResult);
                        return;
                    }
                }
            }
            DoFailedProcess(queryResult);
        }

        /// <summary>
        /// 请添加支付成功后的处理
        /// </summary>
        private void DoSuccessProcess(AlipayF2FQueryResult queryResult)
        {
            //支付成功，请更新相应单据
            Console.WriteLine("扫码支付成功：商户订单号 " + queryResult.response.OutTradeNo);

        }

        /// <summary>
        /// 请添加支付失败后的处理
        /// </summary>
        private void DoFailedProcess(AlipayF2FQueryResult queryResult)
        {
            //支付失败，请更新相应单据
            Console.WriteLine("扫码支付失败：商户订单号 " + queryResult.response.OutTradeNo);
        }

        /// <summary>
        /// 渲染二维码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private Bitmap RenderQrCode(string str)
        {
            QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.L;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, eccLevel))
                {
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {

                        Bitmap bp = qrCode.GetGraphic(20, Color.Black, Color.White,
                            new Bitmap(Path.Combine(_hostingEnvironment.WebRootPath, "images/alipay.png")), 15);
                        return bp;
                    }
                }
            }

        }

        /// <summary>
        /// 构造支付请求数据
        /// </summary>
        /// <param name="orderName">订单名称</param>
        /// <param name="orderAmount">订单金额</param>
        /// <param name="outTradeNo">订单编号</param>
        /// <returns>请求结果集</returns>
        private AlipayTradePrecreateContentBuilder BuildPrecreateContent(string orderName, string orderAmount, string outTradeNo)
        {
            //线上联调时，请输入真实的外部订单号。
            if (string.IsNullOrEmpty(outTradeNo))
            {
                outTradeNo = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "0000" + (new Random()).Next(1, 10000).ToString();
            }

            AlipayTradePrecreateContentBuilder builder = new AlipayTradePrecreateContentBuilder
            {
                //收款账号
                seller_id = APPID,
                //订单编号
                out_trade_no = outTradeNo,
                //订单总金额
                total_amount = orderAmount,
                //参与优惠计算的金额
                //builder.discountable_amount = "";
                //不参与优惠计算的金额
                //builder.undiscountable_amount = "";
                //订单名称
                subject = orderName,
                //自定义超时时间
                timeout_express = "5m",
                //订单描述
                body = "",
                //门店编号，很重要的参数，可以用作之后的营销
                store_id = "test store id",
                //操作员编号，很重要的参数，可以用作之后的营销
                operator_id = "test"
            };

            //传入商品信息详情
            List<Alipay.AopSdk.F2FPay.Model.GoodsInfo> gList = new List<Alipay.AopSdk.F2FPay.Model.GoodsInfo>();
            Alipay.AopSdk.F2FPay.Model.GoodsInfo goods = new Alipay.AopSdk.F2FPay.Model.GoodsInfo
            {
                goods_id = "goods id",
                goods_name = "goods name",
                price = "9.99",
                quantity = "1"
            };
            gList.Add(goods);
            builder.goods_detail = gList;

            //系统商接入可以填此参数用作返佣
            //ExtendParams exParam = new ExtendParams();
            //exParam.sysServiceProviderId = "20880000000000";
            //builder.extendParams = exParam;

            return builder;

        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="tradeno"></param>
        /// <param name="alipayTradeNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Query(string tradeno, string alipayTradeNo)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            AlipayTradeQueryModel model = new AlipayTradeQueryModel
            {
                OutTradeNo = tradeno,
                TradeNo = alipayTradeNo
            };

            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.SetBizModel(model);

            var response = client.Execute(request);
            return Json(response.Body);
        }

        /// <summary>
        /// 订单查询返回结果(参数任选一可查询)
        /// </summary>
        /// <param name="tradeno">订单号</param>
        /// <param name="alipayTradeNo">支付宝交易号</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetQuery(string tradeno, string alipayTradeNo)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            AlipayTradeQueryModel model = new AlipayTradeQueryModel
            {
                OutTradeNo = tradeno,
                TradeNo = alipayTradeNo
            };

            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.SetBizModel(model);

            var response = client.Execute(request);
            return Json(new
            {
                TradeStatus = response.TradeStatus,
                SendPayDate = response.SendPayDate,
                TradeNo = response.TradeNo
            });

        }

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="tradeno">商户订单号</param>
        /// <param name="alipayTradeNo">支付宝交易号</param>
        /// <param name="refundAmount">退款金额</param>
        /// <param name="refundReason">退款原因</param>
        /// <param name="refundNo">退款单号</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Refund(string tradeno, string alipayTradeNo, string refundAmount, string refundReason, string refundNo)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            AlipayTradeRefundModel model = new AlipayTradeRefundModel
            {
                OutTradeNo = tradeno,
                TradeNo = alipayTradeNo,
                RefundAmount = refundAmount,
                RefundReason = refundReason,
                OutRequestNo = refundNo
            };

            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
            request.SetBizModel(model);

            var response = client.Execute(request);
            return Json(response.Body);
        }

        /// <summary>
        /// 退款查询
        /// </summary>
        /// <param name="tradeno">商户订单号</param>
        /// <param name="alipayTradeNo">支付宝交易号</param>
        /// <param name="refundNo">退款单号</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RefundQuery(string tradeno, string alipayTradeNo, string refundNo)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            if (string.IsNullOrEmpty(refundNo))
            {
                refundNo = tradeno;
            }

            AlipayTradeFastpayRefundQueryModel model = new AlipayTradeFastpayRefundQueryModel
            {
                OutTradeNo = tradeno,
                TradeNo = alipayTradeNo,
                OutRequestNo = refundNo
            };

            AlipayTradeFastpayRefundQueryRequest request = new AlipayTradeFastpayRefundQueryRequest();
            request.SetBizModel(model);

            var response = client.Execute(request);
            return Json(response.Body);
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="tradeno">商户订单号</param>
        /// <param name="alipayTradeNo">支付宝交易号</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OrderClose(string tradeno, string alipayTradeNo)
        {
            IAopClient client = new DefaultAopClient(URL, APPID, APP_PRIVATE_KEY, FORMAT, "1.0", "RSA2", ALIPAY_PUBLIC_KEY, CHARSET, false);

            AlipayTradeCloseModel model = new AlipayTradeCloseModel
            {
                OutTradeNo = tradeno,
                TradeNo = alipayTradeNo
            };

            AlipayTradeCloseRequest request = new AlipayTradeCloseRequest();
            request.SetBizModel(model);

            var response = client.Execute(request);
            return Json(response.Body);
        }

        public string GetHtml(string html)//传入网址
        {
            string resultstring = "";
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(html);//这里的url指要获取的数据网址
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                resultstring = reader.ReadToEnd();
            }
            return resultstring;
        }

        /// <summary>
        /// 支付宝返回项
        /// </summary>
        public enum St
        {
            /// <summary>
            /// 支付状态
            /// </summary>
            tradeStatus,
            /// <summary>
            /// 支付时间
            /// </summary>
            sendPayDate,
            /// <summary>
            /// 支付交易号
            /// </summary>
            tradeNo
        }

        /// <summary>
        /// 支付宝交易查询返回值筛选
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public string GetJson(St cs)
        {
            var js = GetHtml("http://localhost:1901/AliPay/GetQuery?tradeno=716");
            JObject jo = (JObject)JsonConvert.DeserializeObject(js);

            string ss = null;
            switch (cs)
            {
                case St.tradeStatus:
                    ss = jo["tradeStatus"].ToString();
                    break;
                case St.sendPayDate:
                    ss = jo["sendPayDate"].ToString();
                    break;
                case St.tradeNo:
                    ss = jo["tradeNo"].ToString();
                    break;
            }
            return ss;
        }

        /// <summary>
        /// 指定Post地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="url">调用地址</param>
        /// <param name="dic">生成参数键对值</param>
        /// <returns></returns>
        public string PostHtml(string url,Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

    }
}