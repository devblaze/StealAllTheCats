﻿using StealAllTheCats.Dtos;

namespace StealAllTheCats.Services.Interfaces;

public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string url);

    Task<Result<byte[]>> GetByteArrayAsync(string url);
}