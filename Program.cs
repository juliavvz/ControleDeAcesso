
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    static Cadastro cadastro = new Cadastro();

    static void Main()
    {
        cadastro.download();

        int opcao;

        do
        {
            Console.WriteLine("\n===== CONTROLE DE ACESSO =====");
            Console.WriteLine("0 - Sair");
            Console.WriteLine("1 - Cadastrar ambiente");
            Console.WriteLine("2 - Consultar ambiente");
            Console.WriteLine("3 - Excluir ambiente");
            Console.WriteLine("4 - Cadastrar usuario");
            Console.WriteLine("5 - Consultar usuario");
            Console.WriteLine("6 - Excluir usuario");
            Console.WriteLine("7 - Conceder permissao");
            Console.WriteLine("8 - Revogar permissao");
            Console.WriteLine("9 - Registrar acesso");
            Console.WriteLine("10 - Consultar logs");
            Console.Write("Opcao: ");

            int.TryParse(Console.ReadLine(), out opcao);

            switch (opcao)
            {
                case 1:
                    Console.Write("ID do ambiente: ");
                    int idA = int.Parse(Console.ReadLine()!);
                    Console.Write("Nome: ");
                    cadastro.adicionarAmbiente(
                        new Ambiente(idA, Console.ReadLine()!));
                    break;

                case 2:
                    Console.Write("ID do ambiente: ");
                    var a = cadastro.pesquisarAmbiente(
                        new Ambiente(int.Parse(Console.ReadLine()!), ""));
                    Console.WriteLine(a == null ? "Nao encontrado" : a.Nome);
                    break;

                case 3:
                    Console.Write("ID do ambiente: ");
                    Console.WriteLine(cadastro.removerAmbiente(
                        new Ambiente(int.Parse(Console.ReadLine()!), ""))
                        ? "Removido!" : "Nao encontrado!");
                    break;

                case 4:
                    Console.Write("ID do usuario: ");
                    int idU = int.Parse(Console.ReadLine()!);
                    Console.Write("Nome: ");
                    cadastro.adicionarUsuario(
                        new Usuario(idU, Console.ReadLine()!));
                    break;

                case 5:
                    Console.Write("ID do usuario: ");
                    var u = cadastro.pesquisarUsuario(
                        new Usuario(int.Parse(Console.ReadLine()!), ""));
                    Console.WriteLine(u == null ? "Nao encontrado" : u.Nome);
                    break;

                case 6:
                    Console.Write("ID do usuario: ");
                    Console.WriteLine(cadastro.removerUsuario(
                        new Usuario(int.Parse(Console.ReadLine()!), ""))
                        ? "Removido!" : "Nao foi possivel remover!");
                    break;

                case 7:
                    Permissao(true);
                    break;

                case 8:
                    Permissao(false);
                    break;

                case 9:
                    Console.Write("ID do ambiente: ");
                    int amb = int.Parse(Console.ReadLine()!);
                    Console.Write("ID do usuario: ");
                    int usu = int.Parse(Console.ReadLine()!);

                    var ambiente = cadastro.pesquisarAmbiente(
                        new Ambiente(amb, ""));
                    var usuario = cadastro.pesquisarUsuario(
                        new Usuario(usu, ""));

                    if (ambiente == null || usuario == null)
                    {
                        Console.WriteLine("Ambiente ou usuario nao encontrado!");
                    }
                    else
                    {
                        bool autorizado = usuario.Ambientes.Any(x => x.Id == amb);

                        ambiente.registrarLog(new Log(
                            DateTime.Now, usuario, autorizado));

                        Console.WriteLine(autorizado
                            ? "Acesso autorizado!"
                            : "Acesso negado!");
                    }
                    break;

                case 10:
                    Console.Write("ID do ambiente: ");
                    var env = cadastro.pesquisarAmbiente(
                        new Ambiente(int.Parse(Console.ReadLine()!), ""));

                    if (env == null)
                    {
                        Console.WriteLine("Ambiente nao encontrado!");
                        break;
                    }

                    Console.WriteLine("1 - Autorizados");
                    Console.WriteLine("2 - Negados");
                    Console.WriteLine("3 - Todos");
                    int filtro = int.Parse(Console.ReadLine()!);

                    foreach (var log in env.Logs)
                    {
                        if (filtro == 3 ||
                            (filtro == 1 && log.TipoAcesso) ||
                            (filtro == 2 && !log.TipoAcesso))
                        {
                            Console.WriteLine(
                                $"{log.DtAcesso} - {log.Usuario.Nome} - " +
                                (log.TipoAcesso ? "Autorizado" : "Negado"));
                        }
                    }
                    break;

                case 0:
                    cadastro.upload();
                    Console.WriteLine("Dados salvos. Saindo...");
                    break;

                default:
                    Console.WriteLine("Opcao invalida!");
                    break;
            }

        } while (opcao != 0);
    }

    static void Permissao(bool conceder)
    {
        Console.Write("ID do ambiente: ");
        int idA = int.Parse(Console.ReadLine()!);

        Console.Write("ID do usuario: ");
        int idU = int.Parse(Console.ReadLine()!);

        var ambiente = cadastro.pesquisarAmbiente(
            new Ambiente(idA, ""));
        var usuario = cadastro.pesquisarUsuario(
            new Usuario(idU, ""));

        if (ambiente == null || usuario == null)
        {
            Console.WriteLine("Usuario ou ambiente nao encontrado!");
            return;
        }

        bool resultado = conceder
            ? usuario.concederPermissao(ambiente)
            : usuario.revogarPermissao(ambiente);

        Console.WriteLine(resultado
            ? "Operacao realizada com sucesso!"
            : "A permissao ja existe ou nao existe!");
    }
}

class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public List<Ambiente> Ambientes { get; set; } = new();

    public Usuario(int id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    public Usuario() { }

    public bool concederPermissao(Ambiente ambiente)
    {
        if (Ambientes.Any(x => x.Id == ambiente.Id))
            return false;

        Ambientes.Add(ambiente);
        return true;
    }

    public bool revogarPermissao(Ambiente ambiente)
    {
        var item = Ambientes.FirstOrDefault(x => x.Id == ambiente.Id);

        if (item == null)
            return false;

        Ambientes.Remove(item);
        return true;
    }
}

class Ambiente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public Queue<Log> Logs { get; set; } = new();

    public Ambiente(int id, string nome)
    {
        Id = id;
        Nome = nome;
    }

    public Ambiente() { }

    public void registrarLog(Log log)
    {
        if (Logs.Count >= 100)
            Logs.Dequeue();

        Logs.Enqueue(log);
    }
}

class Log
{
    public DateTime DtAcesso { get; set; }
    public Usuario Usuario { get; set; }
    public bool TipoAcesso { get; set; }

    public Log(DateTime data, Usuario usuario, bool tipo)
    {
        DtAcesso = data;
        Usuario = usuario;
        TipoAcesso = tipo;
    }

    public Log() { }
}

class Cadastro
{
    public List<Usuario> Usuarios { get; set; } = new();
    public List<Ambiente> Ambientes { get; set; } = new();

    public void adicionarUsuario(Usuario usuario)
    {
        if (pesquisarUsuario(usuario) == null)
            Usuarios.Add(usuario);
        else
            Console.WriteLine("Usuario ja cadastrado!");
    }

    public bool removerUsuario(Usuario usuario)
    {
        var item = pesquisarUsuario(usuario);

        if (item == null || item.Ambientes.Count > 0)
            return false;

        return Usuarios.Remove(item);
    }

    public Usuario? pesquisarUsuario(Usuario usuario)
    {
        return Usuarios.FirstOrDefault(x => x.Id == usuario.Id);
    }

    public void adicionarAmbiente(Ambiente ambiente)
    {
        if (pesquisarAmbiente(ambiente) == null)
            Ambientes.Add(ambiente);
        else
            Console.WriteLine("Ambiente ja cadastrado!");
    }

    public bool removerAmbiente(Ambiente ambiente)
    {
        var item = pesquisarAmbiente(ambiente);

        if (item == null)
            return false;

        foreach (var usuario in Usuarios)
            usuario.revogarPermissao(item);

        return Ambientes.Remove(item);
    }

    public Ambiente? pesquisarAmbiente(Ambiente ambiente)
    {
        return Ambientes.FirstOrDefault(x => x.Id == ambiente.Id);
    }

    public void upload()
    {
        var opcoes = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };

        File.WriteAllText("dados.json",
            JsonSerializer.Serialize(this, opcoes));
    }

    public void download()
    {
        if (!File.Exists("dados.json"))
            return;

        var opcoes = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        var dados = JsonSerializer.Deserialize<Cadastro>(
            File.ReadAllText("dados.json"), opcoes);

        if (dados != null)
        {
            Usuarios = dados.Usuarios;
            Ambientes = dados.Ambientes;
        }
    }
}
