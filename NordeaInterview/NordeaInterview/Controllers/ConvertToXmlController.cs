using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace NordeaInterview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertToXmlController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConvertToXmlController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(ConvertToXmlCommand command, CancellationToken token = default)
        {
            var result = await _mediator.Send(command, token);
            return Ok(result);
        }

        public class ConvertToXmlCommand : IRequest<string>
        {
            public string Text { get; set; }
        }

        public class ConvertToXmlCommandHandler : IRequestHandler<ConvertToXmlCommand, string>
        {
            public async Task<string> Handle(ConvertToXmlCommand request, CancellationToken cancellationToken)
            {
                var stringBuilder = new StringBuilder();

                using (var writer = XmlWriter.Create(stringBuilder))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("text");

                    var sentennces = request.Text.Split('.');
                    foreach (var sentennce in sentennces)
                    {
                        var words = sentennce.Split(' ').OrderBy(x => x);
                        if(words.Any())
                        {
                            writer.WriteStartElement("sentence");
                            foreach (var word in words)
                            {
                                if (string.IsNullOrEmpty(word)) continue;
                                writer.WriteElementString(nameof(word), word.Trim('.',','));
                            }
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                await Task.CompletedTask;
                return stringBuilder.ToString();
            }
        }
    }
}
