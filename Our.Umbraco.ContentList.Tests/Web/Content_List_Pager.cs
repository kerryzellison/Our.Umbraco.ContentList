using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Our.Umbraco.ContentList.DataSources.Listables;
using Our.Umbraco.ContentList.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.UnitTesting.Adapter;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;

namespace Our.Umbraco.ContentList.Tests.Web
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class Content_List_Pager // : BaseWebTest
    {
        private ContentListModel model;
        private ContentListPaging paging;
        private HtmlHelper<ContentListModel> helper;
        private static ViewContext viewContext;
        private HttpContextBase httpContext;

        private UmbracoSupport umbracoSupport;
        
        [SetUp]
        public void SetUp()
        {
            umbracoSupport = new UmbracoSupport();
            umbracoSupport.SetupUmbraco();

            Current.UmbracoContext.PublishedRequest = new FakePublishedRequest(Mock.Of<IPublishedRouter>(), Current.UmbracoContext, new Uri("http://localhost"));

            var queryA = new ContentListConfiguration
            {
                DataSource = new ContentListDataSource
                {
                    Type = typeof(ListableByNodeDataSource).GetFullNameWithAssembly()
                },
                PageSize = 5,
                Skip = 2
            };

            var queryB = new ContentListConfiguration
            {
                DataSource = new ContentListDataSource
                {
                    Type = typeof(ListableChildrenDataSource).GetFullNameWithAssembly(),
                    Parameters = new List<DataSourceParameterValue>
                    {
                        new DataSourceParameterValue { Key = "sort", Value = "sortorder" }
                    }
                },
                PageSize = 10
            };

            Console.WriteLine(queryB.CreateHash());

            paging = new ContentListPaging
            {
                From = 1,
                To = 5,
                Page = 1,
                PageSize = 5,
                Total = 5,
                ShowPaging = true
            };

            model = new ContentListModel
            {
                Paging = paging,
                Hash = queryA.CreateHash()
            };

            helper = CreateHelper(model, queryB.CreateHash() + "=3");
        }

        [TearDown]
        public void TearDown()
        {
            umbracoSupport.DisposeUmbraco();
        }

        [Test]
        public void Is_Empty_When_Only_One_Page()
        {
            var html = helper.ContentListPager();

            Assert.AreEqual("", html.ToString());
        }

        [Test]
        public void Is_Empty_When_Show_Paging_Is_False()
        {
            paging.Total = 10;
            paging.ShowPaging = false;

            var html = helper.ContentListPager();

            Assert.AreEqual("", html.ToString());
        }

        [Test]
        [TestCase(6, 2)]
        [TestCase(9, 2)]
        [TestCase(10, 2)]
        [TestCase(11, 3)]
        public void Adds_Link_With_QueryString_For_Each_Page(int total, int expectedPages)
        {
            paging.Total = total;
            paging.Page = 2;

            var html = helper.ContentListPager();

            var expectedOutput = "<div class=\"pagination \">";
            for (var i = 0; i < expectedPages; i++)
                if (i == 1)
                    expectedOutput += String.Format("<span>{0}</span>", i + 1);
                else
                    expectedOutput += String.Format("<a href=\"?DG6yG3TgWzjdrXXPtmly9Q=3&KRSBHTlka609c5qhzeZzgQ={0}\">{0}</a>", i + 1);
            expectedOutput += "</div>";

            Assert.AreEqual(expectedOutput, html.ToString());

            Console.WriteLine("helper.ContentListPager()");
            Console.WriteLine(html.ToString());
        }

        [Test]
        [TestCase("http://some.page.com/some/url/")]
        [TestCase("http://localhost/list/")]
        [TestCase("http://localhost/list/?DG6yG3TgWzjdrXXPtmly9Q=3")]
        [TestCase("http://localhost/list")]
        public void Adds_Current_Url_As_Link(string expectedUrl)
        {
            var url = new Uri(expectedUrl);

            Mock.Get(helper.ViewContext.HttpContext.Request).Setup(r => r.Url).Returns(url);

            paging.Total = 10;

            var html = helper.ContentListPager();

            var match = Contains.Substring(expectedUrl);
            Assert.That(html.ToString(), match);
        }

        [Test]
        public void Decorates_Pages_With_Elements_And_Classes_From_Optional_Params()
        {
            paging.Total = 10;
            var html = helper.ContentListPager("fancy-pager", "item", "anchor", "ul", "li");
            Assert.AreEqual(
                "<ul class=\"pagination fancy-pager\">" +
                "<li class=\"item active\"><span>1</span></li>" +
                "<li class=\"item\"><a class=\"anchor\" href=\"?DG6yG3TgWzjdrXXPtmly9Q=3&KRSBHTlka609c5qhzeZzgQ=2\">2</a></li>" +
                "</ul>",
                html.ToString()
                );

            Console.WriteLine("helper.ContentListPager(\"fancy-pager\", \"item\", \"anchor\", \"ul\", \"li\")");
            Console.WriteLine(html.ToString());
        }

        private HtmlHelper<ContentListModel> CreateHelper(ContentListModel model, string otherParams)
        {
            httpContext = Mock.Of<HttpContextBase>();
            var request = Mock.Of<HttpRequestBase>();
            var qs = new NameValueCollection();
            otherParams.Split('&').Select(x => x.Split('=')).ToList().ForEach(x => qs.Add(x[0], x[1]));
            Mock.Get(request).Setup(x => x.QueryString).Returns(qs);
            Mock.Get(httpContext).Setup(x => x.Request).Returns(request);
            var viewData = new ViewDataDictionary<ContentListModel>(model);
            var viewPage = new ViewPage {ViewData = viewData};
            viewContext = new ViewContext {HttpContext = httpContext, RequestContext = new RequestContext(httpContext, new RouteData())};
            var helper = new HtmlHelper<ContentListModel>(viewContext, viewPage);
            return helper;
        }
    }
}
