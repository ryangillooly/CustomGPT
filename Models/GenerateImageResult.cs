using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatGptTest.Models;

public record GenerateImageResult
(
  string Created,
  [property: JsonProperty("data")] List<Result> Results
);

public record Result(string Url);
