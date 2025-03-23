﻿using StealAllTheCats.Models;

namespace StealAllTheCats.Dtos.Results;

public class CatPaginatedResult
{
    public int TotalItems { get; set; }
    public List<CatEntity>? Cats { get; set; }
}