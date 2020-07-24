using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NordeaInterview.Properties;

namespace NordeaInterview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertToCsvController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConvertToCsvController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(ConvertToCsvCommand command, CancellationToken token = default)
        {
            var result = await _mediator.Send(command, token);
            return Ok(result);
        }

        public class ConvertToCsvCommand : IRequest<string>
        {
            public string Text { get; set; }
        }

        public class ConvertToCsvCommandHandler : IRequestHandler<ConvertToCsvCommand, string>
        {
            public async Task<string> Handle(ConvertToCsvCommand request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Text))
                {
                    return Resource.XmlError;
                }

                await using var memory = new MemoryStream();
                await using var writer = new StreamWriter(memory);
                await using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture)
                {
                    Configuration =
                    {
                        Delimiter = ",",
                        HasHeaderRecord = true
                    }
                };

                var sentences = request.Text.Split('.').Where(words => !string.IsNullOrEmpty(words)).ToList();
                var maxWords = sentences.Max(x => x.Split(' ').Count());
                for (var i = 1; i < maxWords; i++)
                {
                    csvWriter.WriteField($"{Resource.Word} {i}");
                }
                await csvWriter.NextRecordAsync();

                for (var i = 0; i < sentences.Count(); i++)
                {
                    var words = sentences[i].Split(' ').OrderBy(x => x).ToList();
                    if (words.Any())
                    {
                        csvWriter.WriteField($"{Resource.Sentence} {i+1}");
                        foreach (var word in words.Where(word => !string.IsNullOrEmpty(word)))
                        {
                            csvWriter.WriteField(word.Trim('.', ','));
                        }

                        await csvWriter.NextRecordAsync();
                    }
                }

                await writer.FlushAsync();
                return Encoding.UTF8.GetString(memory.ToArray());
            }
        }
    }
}
