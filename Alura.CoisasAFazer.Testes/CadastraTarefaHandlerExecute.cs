using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Services.Handlers;
using System;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoBd()
        {
            //arrange

            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"),new DateTime(2022,12,31));
            var repo = new RepositorioFake();
            var handler = new CadastraTarefaHandler(repo);

            //act
            handler.Execute(comando);

            //assert
            Assert.True(true);

        }
    }
}
