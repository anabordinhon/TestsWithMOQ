using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class GerenciaPrazoDasTarefasHandlerExecute
    {
        [Fact]
        public void QuandoTarefasEstiveremAtrasadasDevemMudarSeuStatus()
        {
            //arrange
            var compCateg = new Categoria(1, "Compras");

            var tarefas = new List<Tarefa>
            {
                new Tarefa(1,"Comprar Presente pro João",compCateg, new DateTime(2018,10,8),null,StatusTarefa.Criada),
                new Tarefa(2,"Comprar Presente pro Daniel",compCateg, new DateTime(2018,10,10),null,StatusTarefa.Criada)
            };

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            repo.IncluirTarefas(tarefas.ToArray());

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019,01,01));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //act
            handler.Execute(comando);

            //assert
            var tarefasEmAtraso = repo.ObtemTarefas(t => t.Status == StatusTarefa.EmAtraso);
            Assert.Equal(2, tarefasEmAtraso.Count());
        }

        [Fact]
        public void QuandoInvocadoDeveChamarAtualizarTarefasNaQtdeVezesDoTotalDeTarefasAtrasadas()
        {
            //arrange
            var categoria = new Categoria("categoria");
            var tarefas = new List<Tarefa> {
                new Tarefa(1, "Comprar Presente pro João",categoria, new DateTime(2018, 10, 8), null, StatusTarefa.Criada),
                new Tarefa(2, "Comprar Presente pro Daniel",categoria, new DateTime(2018, 10, 10), null, StatusTarefa.Criada),
                new Tarefa(3, "Comprar Presente pro Murilo",categoria, new DateTime(2018, 10, 15), null, StatusTarefa.Criada)
                };
            var mock = new Mock<IRepositorioTarefas>();
            //será um stub, pois serão injetadas informações para o repositorio
            mock.Setup(r => r.ObtemTarefas(It.IsAny<Func<Tarefa, bool>>())).Returns(tarefas);
            var repo = mock.Object;

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019, 01, 01));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //act
            handler.Execute(comando);

            //assert
            mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Once());
        }
    }
}
