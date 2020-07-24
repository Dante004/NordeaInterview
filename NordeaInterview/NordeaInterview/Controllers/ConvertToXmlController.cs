using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NordeaInterview.Properties;

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
                if (string.IsNullOrEmpty(request.Text))
                {
                    return Resource.XmlError;
                }

                var stringBuilder = new StringBuilder();

                using (var writer = XmlWriter.Create(stringBuilder))
                {
                    writer.WriteStartElement(Resource.Text);

                    var sentences = request.Text.Split('.')
                        .Where(words => !string.IsNullOrEmpty(words));

                    foreach (var sentence in sentences)
                    {
                        var words = sentence.Split(' ')
                            .OrderBy(x => x)
                            .Where(word => !string.IsNullOrEmpty(word)).ToList();

                        if(words.Any())
                        {
                            writer.WriteStartElement(Resource.Sentence);
                            foreach (var word in words)
                            {
                                writer.WriteElementString(Resource.Word, word.Trim('.',','));
                            }
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }
                await Task.CompletedTask;
                return stringBuilder.ToString();
            }
        }
    }
}
