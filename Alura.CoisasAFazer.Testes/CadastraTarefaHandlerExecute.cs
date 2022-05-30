using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoBd()
        {
            //arrange

            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"),new DateTime(2022,12,31));

            //usar moq para passar o objeto de log
            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo,mock.Object);

            //act
            handler.Execute(comando);

            //assert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar Xunit").FirstOrDefault();
            Assert.NotNull(tarefa);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalse()
        {
            //arrange

            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2022, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();
            //It.IsAny (qualquer argumento de entrada do tipo Tarefa)
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um erro na inclusão de tarefas")); //comportamento do mock
            var repo = mock.Object;
            //O 1º código configura o repositório simulado para lançar uma exceção ao chamarmos IncluirTarefa()
            //com qualquer array de tarefas. O 2º código pega uma instância do tipo simulado para ser usado no
            //CadastrarTarefasHandler.
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            CommandResult resultado =  handler.Execute(comando);

            //assert
            Assert.False(resultado.IsSuccess);
        }
        delegate void CapturaMensagemLog(LogLevel leve, EventId eventId, object state, Exception exception,
            Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInfoValidaDeveLogar()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2022, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCaptura = string.Empty;
            CapturaMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCaptura = func(state, exception);
            };
            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception,
                    string>>())
                ).Callback(captura);

            var mock = new Mock<IRepositorioTarefas>();
            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //act
            handler.Execute(comando);

            //assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains("Estudar Xunit", mensagemCaptura);

        }
        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMsgDeExcecao()
        {
            //arrange
            var msgErroEsperada = "Houve um erro";
            var excecaoEsperada = new Exception("Houve um erro na inclusão de tarefas");
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2022, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(excecaoEsperada); //comportamento do mock
            var repo = mock.Object;
            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            CommandResult resultado = handler.Execute(comando);

            //assert
            //sempre explicitar as dependencias para facilitar o teste
            mockLogger.Verify(l => l.Log(LogLevel.Error,It.IsAny<EventId>(),It.IsAny<object>(),excecaoEsperada,It.IsAny<Func<object,Exception,string>>()), 
                Times.Once());

        }
    }
}
