using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using Moq;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoBd()
        {
            //arrange

            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"),new DateTime(2022,12,31));

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo);

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
            var mock = new Mock<IRepositorioTarefas>();
            //It.IsAny (qualquer argumento de entrada do tipo Tarefa)
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um erro na inclusão de tarefas")); //comportamento do mock
            var repo = mock.Object;
            //O 1º código configura o repositório simulado para lançar uma exceção ao chamarmos IncluirTarefa()
            //com qualquer array de tarefas. O 2º código pega uma instância do tipo simulado para ser usado no
            //CadastrarTarefasHandler.
            var handler = new CadastraTarefaHandler(repo);

            //act
            CommandResult resultado =  handler.Execute(comando);

            //assert
            Assert.False(resultado.IsSuccess);
        }
    }
}
