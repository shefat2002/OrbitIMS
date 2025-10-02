﻿using System.ComponentModel.DataAnnotations;

namespace OrbitIMS.Data
{
    public class Category : BaseEntity
    {
        [Display(Name = "Category")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Product>? Products { get; set; }
    }
}
