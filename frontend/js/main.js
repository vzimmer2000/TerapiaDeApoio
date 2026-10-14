const API_BASE = "";

const servicosLista = document.getElementById("servicos-lista");
const pacienteSelect = document.getElementById("agendamento-paciente");
const servicoSelect = document.getElementById("agendamento-servico");
const profissionalSelect = document.getElementById("agendamento-profissional");
const dataHoraInput = document.getElementById("agendamento-data");

const pacienteForm = document.getElementById("form-paciente");
const agendamentoForm = document.getElementById("form-agendamento");

const pacienteFeedback = document.getElementById("paciente-feedback");
const agendamentoFeedback = document.getElementById("agendamento-feedback");
const sistemaFeedback = document.getElementById("sistema-feedback");

const iconesServicos = ["✳", "◎", "↗", "◇", "✦", "○"];

// Requisições HTTP e tratamento básico de erros.

async function apiRequest(endpoint, options = {}) {
    const headers = {
        ...(options.headers ?? {})
    };

    if (options.body) {
        headers["Content-Type"] = "application/json; charset=utf-8";
    }

    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers
    });

    if (response.status === 204) {
        return null;
    }

    const texto = await response.text();

    let data = null;

    if (texto) {
        try {
            data = JSON.parse(texto);
        } catch {
            data = { detail: texto };
        }
    }

    if (!response.ok) {
        const mensagensValidacao = data?.errors
            ? Object.values(data.errors).flat().join(" ")
            : null;

        const mensagem =
            data?.detail ||
            mensagensValidacao ||
            data?.title ||
            `A requisição falhou (HTTP ${response.status}).`;

        throw new Error(mensagem);
    }

    return data;
}

function mostrarFeedback(elemento, mensagem, tipo = "info") {
    elemento.textContent = mensagem;
    elemento.className = `feedback feedback--${tipo}`;
}

function limparFeedback(elemento) {
    elemento.textContent = "";
    elemento.className = "feedback";
}

function configurarBotao(formulario, textoPadrao, carregando) {
    const botao = formulario.querySelector('button[type="submit"]');

    botao.disabled = carregando;
    botao.textContent = carregando ? "Processando..." : textoPadrao;
}

// Serviços.

function renderizarServicos(servicos) {
    servicosLista.replaceChildren();

    if (servicos.length === 0) {
        const mensagem = document.createElement("p");
        mensagem.className = "empty-message";
        mensagem.textContent = "Nenhum serviço ativo foi cadastrado.";
        servicosLista.appendChild(mensagem);
        return;
    }

    servicos.forEach((servico, indice) => {
        const card = document.createElement("article");
        card.className = "service-card";

        const icone = document.createElement("div");
        icone.className = "service-icon";
        icone.setAttribute("aria-hidden", "true");
        icone.textContent = iconesServicos[indice % iconesServicos.length];

        const titulo = document.createElement("h3");
        titulo.textContent = servico.nome;

        const descricao = document.createElement("p");
        descricao.textContent =
            servico.descricao || "Atendimento oferecido pela clínica.";

        const duracao = document.createElement("div");
        duracao.className = "service-duration";
        duracao.textContent = `Duração: ${servico.duracaoMinutos} minutos`;

        card.append(icone, titulo, descricao, duracao);
        servicosLista.appendChild(card);
    });
}

// Preenchimento dos campos de seleção.

function preencherSelect(select, itens, textoInicial, obterTexto) {
    select.replaceChildren();

    const opcaoInicial = document.createElement("option");
    opcaoInicial.value = "";
    opcaoInicial.disabled = true;
    opcaoInicial.selected = true;
    opcaoInicial.textContent = textoInicial;

    select.appendChild(opcaoInicial);

    itens.forEach((item) => {
        const opcao = document.createElement("option");
        opcao.value = String(item.id);
        opcao.textContent = obterTexto(item);

        select.appendChild(opcao);
    });
}

// Carregamento dos dados existentes na API.

async function carregarServicos() {
    const servicos = await apiRequest("/api/servicos");

    const ativos = servicos.filter((servico) => servico.ativo);

    renderizarServicos(ativos);

    preencherSelect(
        servicoSelect,
        ativos,
        "Selecione um serviço",
        (servico) => `${servico.nome} (${servico.duracaoMinutos} min)`
    );
}

async function carregarProfissionais() {
    const profissionais = await apiRequest("/api/profissionais");

    const ativos = profissionais.filter(
        (profissional) => profissional.ativo
    );

    preencherSelect(
        profissionalSelect,
        ativos,
        "Selecione um profissional",
        (profissional) =>
            `${profissional.nome} — ${profissional.especialidade}`
    );
}

async function carregarPacientes(idSelecionado = null) {
    const pacientes = await apiRequest("/api/pacientes");

    preencherSelect(
        pacienteSelect,
        pacientes,
        pacientes.length > 0
            ? "Selecione um paciente"
            : "Cadastre um paciente primeiro",
        (paciente) => paciente.nome
    );

    if (idSelecionado !== null) {
        pacienteSelect.value = String(idSelecionado);
    }

    return pacientes;
}

async function carregarDadosIniciais() {
    sistemaFeedback.textContent = "Conectando à API...";

    try {
        await Promise.all([
            carregarServicos(),
            carregarProfissionais(),
            carregarPacientes()
        ]);

        sistemaFeedback.textContent =
            "Dados carregados. A interface está conectada à API.";
    } catch (erro) {
        sistemaFeedback.textContent =
            `Não foi possível carregar os dados: ${erro.message}`;

        console.error("Falha ao carregar os dados da API:", erro);
    }
}

// Cadastro de paciente.

pacienteForm.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limparFeedback(pacienteFeedback);

    const botaoTexto = "Cadastrar paciente";

    configurarBotao(pacienteForm, botaoTexto, true);

    const body = {
        nome: document.getElementById("paciente-nome").value.trim(),
        telefone: document.getElementById("paciente-telefone").value.trim(),
        email:
            document.getElementById("paciente-email").value.trim() || null
    };

    try {
        const paciente = await apiRequest("/api/pacientes", {
            method: "POST",
            body: JSON.stringify(body)
        });

        await carregarPacientes(paciente.id);

        pacienteForm.reset();

        mostrarFeedback(
            pacienteFeedback,
            `Paciente ${paciente.nome} cadastrado com sucesso. Ele já está selecionado para o agendamento.`,
            "success"
        );
    } catch (erro) {
        mostrarFeedback(
            pacienteFeedback,
            erro.message,
            "error"
        );
    } finally {
        configurarBotao(pacienteForm, botaoTexto, false);
    }
});

// Criação do agendamento.

agendamentoForm.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limparFeedback(agendamentoFeedback);

    const botaoTexto = "Confirmar agendamento";

    configurarBotao(agendamentoForm, botaoTexto, true);

    const dataHoraInicio = dataHoraInput.value;

    if (!dataHoraInicio) {
        mostrarFeedback(
            agendamentoFeedback,
            "Selecione uma data e um horário.",
            "error"
        );

        configurarBotao(agendamentoForm, botaoTexto, false);
        return;
    }

    const body = {
        pacienteId: Number(pacienteSelect.value),
        profissionalId: Number(profissionalSelect.value),
        servicoId: Number(servicoSelect.value),
        dataHoraInicio
    };

    try {
        const agendamento = await apiRequest("/api/agendamentos", {
            method: "POST",
            body: JSON.stringify(body)
        });

        const inicio = new Date(
            agendamento.dataHoraInicio
        ).toLocaleString("pt-BR", {
            dateStyle: "short",
            timeStyle: "short"
        });

        const fim = new Date(
            agendamento.dataHoraFim
        ).toLocaleString("pt-BR", {
            timeStyle: "short"
        });

        mostrarFeedback(
            agendamentoFeedback,
            `Agendamento #${agendamento.id} confirmado para ${inicio}. Término previsto: ${fim}.`,
            "success"
        );

        agendamentoForm.reset();
    } catch (erro) {
        mostrarFeedback(
            agendamentoFeedback,
            erro.message,
            "error"
        );
    } finally {
        configurarBotao(agendamentoForm, botaoTexto, false);
    }
});

// Impede que o navegador ofereça horários anteriores ao momento atual.

function configurarDataMinima() {
    const agora = new Date();

    agora.setMinutes(
        agora.getMinutes() - agora.getTimezoneOffset()
    );

    dataHoraInput.min = agora.toISOString().slice(0, 16);
}

configurarDataMinima();
carregarDadosIniciais();