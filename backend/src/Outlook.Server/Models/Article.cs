﻿using Outlook.Server.Models.Interfaces;
using Outlook.Server.Models.Relations;
using Outlook.Server.Validation_Attributes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Outlook.Server.Models
{
    public class Article : IArticle, IRatedBlog
    {

        public int Id { get; set; }

        public Language Language { get; set; }

        public int CategoryID { get; set; }

        public Category Category { get; set; }

        public int IssueID { get; set; }

        public Issue Issue { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        [NotMapped]
        public IFormFile Picture { get; set; }

        [Display(Name = "Picture")]
        public string PicturePath { get; set; }

        [NotMapped]
        [DisplayName("Delete Picture")]
        public bool DeletePicture { get; set; }

        public string Text { get; set; }

        public int MemberID { get; set; }

        public Member Member { get; set; }

        [NotMapped]
        [DisplayName("New Writer")]
        [MemberUniqueness]
        // TODO: If writer already exists assign the new article to him
        public string NewWriter { get; set; }

        public DateTime DateTime { get; set; }

        public int Rate { get; set; }

        public int NumberOfVotes { get; set; }

        public int NumberOfFavorites { get; set; }

        public List<Comment> Comments { get; set; }

        public List<UserFavoritedArticleRelation> Favorites { get; set; }

        public List<UserRateArticle> Rates { get; set; }

        public void RateDown()
        {
            Rate--;
            NumberOfVotes++;
        }

        public void RateUp()
        {
            Rate++;
            NumberOfVotes++;
        }

        public void UnRateDown()
        {
            Rate++;
            NumberOfVotes--;
        }

        public void UnRateUp()
        {
            Rate--;
            NumberOfVotes--;
        }

        public Article SetLanguage(Language language)
        {
            Language = language;
            return this;
        }

        public Article SetCategory(Category category)
        {
            Category = category;
            return this;
        }

        public Article SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public Article SetText(string text)
        {
            Text = text;
            return this;
        }

        public Article SetSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }
    }
}
