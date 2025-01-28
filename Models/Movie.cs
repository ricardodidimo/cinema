namespace cinema.Models;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Movie
{
    public List<Player> RoundVotes { get; set; } = new List<Player>();

    [JsonProperty("adult")]
    public bool Adult { get; set; }

    [JsonProperty("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonProperty("genre_ids")]
    public required List<int> GenreIds { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("original_language")]
    public required string OriginalLanguage { get; set; }

    [JsonProperty("original_title")]
    public required string OriginalTitle { get; set; }

    [JsonProperty("overview")]
    public required string Overview { get; set; }

    [JsonProperty("popularity")]
    public double Popularity { get; set; }

    [JsonProperty("poster_path")]
    public string? PosterPath { get; set; }

    [JsonProperty("release_date")]
    public DateTime ReleaseDate { get; set; }

    [JsonProperty("title")]
    public required string Title { get; set; }

    [JsonProperty("video")]
    public bool Video { get; set; }

    [JsonProperty("vote_average")]
    public double VoteAverage { get; set; }

    [JsonProperty("vote_count")]
    public int VoteCount { get; set; }
}
