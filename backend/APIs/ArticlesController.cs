﻿using backend.Data;
using backend.Hubs;
using backend.Models;
using backend.Models.Relations;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static backend.Models.Relations.UserRateArticle;

namespace backend.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly OutlookContext context;
        private readonly ArticleService articleService;
        private readonly IdentityService identityService;
        private readonly IWebHostEnvironment env;
        private readonly IHubContext<ArticleHub, IArticleHub> hubContext;
        private readonly Logger.Logger logger;
        public IConfiguration configuration { get; }

        public ArticlesController(
            OutlookContext context,
            ArticleService articleService,
            IdentityService identityService,
            IHubContext<ArticleHub, IArticleHub> articlehub,
            IWebHostEnvironment env, IConfiguration configuration)
        {
            this.context = context;
            this.articleService = articleService;
            this.identityService = identityService;
            this.env = env;
            this.configuration = configuration;
            hubContext = articlehub;
            logger = Logger.Logger.Instance(Logger.Logger.LogField.userArticles);
        }

        // GET: api/Articles
        [HttpGet("{issueID}")]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles(int issueID)
        {
            // Get articles in an issue
            var articles = from article in context.Article
                           where article.IssueID == issueID
                           select article;

            // Foreach article, add its info
            foreach (var article in articles)
            {
                await articleService.GetArticleProperties(article);
            }

            return await articles.ToListAsync();
        }

        // GET: api/Articles/Article/5
        [HttpGet("Article/{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await context.Article.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }
            await articleService.GetArticleProperties(article);

            return article;
        }

        // GET: api/Articles
        [HttpGet]
        public ActionResult GetTopArticles()
        {
            var topRatedArticles = from article in context.Article
                                   orderby article.Rate
                                   descending
                                   select article;

            var topFavoritedArticles = from article in context.Article
                                       orderby article.NumberOfFavorites
                                       descending
                                       select article;

            var shortListedTopRatedArticles = GetShortlistedTopArticles(topRatedArticles);
            var shortListedTopFavoritedArticles = GetShortlistedTopArticles(topFavoritedArticles);

            return Ok(new
            {
                topRatedArticles = shortListedTopRatedArticles,
                topFavoritedArticles = shortListedTopFavoritedArticles
            });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("RateUpArticle/{articleID}")]
        public async Task<ActionResult> RateUpArticle(int articleID)
        {
            // TODO: Fix bug when the article gets its first vote
            var user = await identityService.GetUserWithToken(HttpContext);
            var userRateArticle = await context.UserRateArticle.FirstOrDefaultAsync(u => (u.Article.Id == articleID) && (u.User.Id == user.Id));
            var article = await context.Article.FindAsync(articleID);

            if (userRateArticle != null)
            {
                if (userRateArticle.Rate == UserRate.Up)
                {
                    return Accepted("You've already rated up this article.");
                }
                else
                {
                    if (userRateArticle.Rate == UserRate.Down)
                    {
                        article.UnRateDown();
                    }
                    userRateArticle.Rate = UserRate.Up;
                }
            }
            else
            {
                // Rate down the article
                context.UserRateArticle.Add(new UserRateArticle { User = user, Article = article, Rate = UserRate.Down });
            }
            article.RateUp();
            await context.SaveChangesAsync();

            // Notify all clients
            await hubContext.Clients.All.ArticleScoreChange(article.Id, article.Rate, article.NumberOfVotes);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("RateDownArticle/{articleID}")]
        public async Task<ActionResult> RateDownArticle(int articleID)
        {
            var user = await identityService.GetUserWithToken(HttpContext);
            var userRateArticle = await context.UserRateArticle.FirstOrDefaultAsync(u => (u.Article.Id == articleID) && (u.User.Id == user.Id));
            var article = await context.Article.FindAsync(articleID);

            if (userRateArticle != null)
            {
                if (userRateArticle.Rate == UserRate.Down)
                {
                    return Accepted("You've already rated down this article.");
                }
                else
                {
                    if (userRateArticle.Rate == UserRate.Up)
                    {
                        article.UnRateUp();
                    }
                    userRateArticle.Rate = UserRate.Down;
                }
            }
            else
            {
                // Rate down the article
                context.UserRateArticle.Add(new UserRateArticle { User = user, Article = article, Rate = UserRate.Down });
            }
            article.RateDown();
            await context.SaveChangesAsync();

            // Notify all clients
            await hubContext.Clients.All.ArticleScoreChange(article.Id, article.Rate, article.NumberOfVotes);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPut("FavoriteArticle/{articleID}")]
        public async Task<ActionResult> FavoriteArticle(int articleID)
        {
            var user = await identityService.GetUserWithToken(HttpContext);
            var userFavoritedArticle = await context.UserFavoritedArticleRelation.FirstOrDefaultAsync(u => (u.Article.Id == articleID) && (u.User.Id == user.Id));
            var article = await context.Article.FindAsync(articleID);

            if (userFavoritedArticle != null)
            {
                context.UserFavoritedArticleRelation.Remove(userFavoritedArticle);
                article.NumberOfFavorites--;
            }
            else
            {
                context.UserFavoritedArticleRelation.Add(new UserFavoritedArticleRelation { User = user, Article = article });
                article.NumberOfFavorites++;
            }
            await context.SaveChangesAsync();

            // Notify all clients
            await hubContext.Clients.All.ArticleFavoriteChange(article.Id, article.NumberOfFavorites);

            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("GetUserFavorites")]
        public async Task<ActionResult> GetUserFavorites()
        {
            var user = await identityService.GetUserWithToken(HttpContext);
            var userFavoriteArticles = from userFavoritedArticle in context.UserFavoritedArticleRelation
                                       where userFavoritedArticle.User == user
                                       select userFavoritedArticle.Article;

            foreach (var article in userFavoriteArticles)
            {
                await articleService.GetArticleProperties(article);
            }

            return Ok(userFavoriteArticles);
        }


        [HttpPost("Upload")]
        public async Task<ActionResult> UploadArticle(IFormCollection file)
        {
            if (file.Files.Count != 0)
            {
                var document = file.Files.ElementAt(0);
                var extension = document.FileName.Substring(document.FileName.LastIndexOf('.'));

                // Add unique name to avoid possible name conflicts
                var uniqueDocumentName = DateTime.Now.Ticks.ToString() + $".{extension}";
                var articleDocumentsFolderPath = Path.Combine(new string[] { env.WebRootPath, "docs", "Articles\\" });
                var articleDocumentFilePath = Path.Combine(articleDocumentsFolderPath, uniqueDocumentName);

                if (!Directory.Exists(articleDocumentsFolderPath))
                {
                    Directory.CreateDirectory(articleDocumentsFolderPath);
                }

                using (var fileStream = new FileStream(articleDocumentFilePath, FileMode.Create, FileAccess.Write))
                {
                    if (file.Files.Count != 0)
                    {
                        // Copy the photo to storage
                        await document.CopyToAsync(fileStream);
                    }
                }

                var baseUrl = configuration["ApplicationUrls:Server"];
                var ArticleDocumentFullPath = $"{baseUrl}/docs/Articles/{uniqueDocumentName}";
                logger.Log(ArticleDocumentFullPath);
            }

            return Ok();
        }

        private IEnumerable<Article> GetShortlistedTopArticles(IOrderedQueryable<Article> articles)
        {
            var englishShortlistedArticles = articles.Where(a => a.Language == Models.Interfaces.Language.English).AsEnumerable().Take(3);
            var arabicShortlistedArticles = articles.Where(a => a.Language == Models.Interfaces.Language.Arabic).AsEnumerable().Take(3);

            return englishShortlistedArticles.Concat(arabicShortlistedArticles);
        }
    }
}
