const API_BASE = "";

const servicosLista = document.getElementById("servicos-lista");
const pacienteSelect = document.getElementById("agendamento-paciente");
const servicoSelect = document.getElementById("agendamento-servico");
const profissionalSelect = document.getElementById("agendamento-profissional");

const dataInput = document.getElementById("agendamento-data");
const horarioSelect = document.getElementById("agendamento-horario");
const horariosFeedback = document.getElementById("horarios-feedback");

const pacienteForm = document.getElementById("form-paciente");
const agendamentoForm = document.getElementById("form-agendamento");

const pacienteFeedback = document.getElementById("paciente-feedback");
const agendamentoFeedback = document.getElementById("agendamento-feedback");
const sistemaFeedback = document.getElementById("sistema-feedback");

const iconesServicos = ["✳", "◎", "↗", "◇", "✦", "○"];

const consultaAgendamentosForm = document.getElementById(
    "form-consultar-agendamentos"
);

const consultaTelefoneInput = document.getElementById(
    "consulta-telefone"
);

const meusAgendamentosLista = document.getElementById(
    "meus-agendamentos-lista"
);

const meusAgendamentosFeedback = document.getElementById(
    "meus-agendamentos-feedback"
);



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

async function carregarProfissionais(
    servicoId = null,
    profissionalSelecionado = null
) {
    const endpoint = servicoId
        ? `/api/profissionais?servicoId=${encodeURIComponent(servicoId)}`
        : "/api/profissionais";

    const profissionais = await apiRequest(endpoint);

    const ativos = profissionais.filter(
        (profissional) => profissional.ativo
    );

    const textoInicial = ativos.length > 0
        ? "Selecione um profissional"
        : servicoId
            ? "Nenhum profissional oferece este serviço"
            : "Nenhum profissional cadastrado";

    preencherSelect(
        profissionalSelect,
        ativos,
        textoInicial,
        (profissional) =>
            `${profissional.nome} — ${profissional.especialidade}`
    );

    // Preserva a seleção se o profissional continuar na lista filtrada.
    if (
        profissionalSelecionado &&
        ativos.some(
            (profissional) =>
                String(profissional.id) === String(profissionalSelecionado)
        )
    ) {
        profissionalSelect.value = String(profissionalSelecionado);
    }

    return ativos;
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
let consultaHorariosAtual = 0;

async function carregarHorariosDisponiveis() {
    const numeroConsulta = ++consultaHorariosAtual;

    const data = dataInput.value;
    const profissionalId = profissionalSelect.value;
    const servicoId = servicoSelect.value;

    const opcaoInicial = document.createElement("option");
    opcaoInicial.value = "";
    opcaoInicial.disabled = true;
    opcaoInicial.selected = true;
    opcaoInicial.textContent = "Selecione um horário";

    horarioSelect.replaceChildren(opcaoInicial);
    horarioSelect.disabled = true;

    limparFeedback(horariosFeedback);

    const camposPendentes = [];

    if (!servicoId) camposPendentes.push("serviço");
    if (!profissionalId) camposPendentes.push("profissional");
    if (!data) camposPendentes.push("data");

    if (camposPendentes.length > 0) {
        if (servicoId || profissionalId || data) {
            mostrarFeedback(
                horariosFeedback,
                `Selecione ${camposPendentes.join(", ")} para consultar os horários.`,
                "info"
            );
        }

        return;
    }

    mostrarFeedback(
        horariosFeedback,
        "Consultando horários disponíveis...",
        "info"
    );

    const parametros = new URLSearchParams({
        profissionalId,
        servicoId,
        data
    });

    try {
        const horarios = await apiRequest(
            `/api/agendamentos/disponibilidade?${parametros.toString()}`
        );

        // Ignora respostas antigas caso o usuário altere os campos
        // enquanto uma consulta anterior ainda estiver em andamento.
        if (numeroConsulta !== consultaHorariosAtual) {
            return;
        }

        if (!Array.isArray(horarios)) {
            throw new Error("A API retornou uma lista de horários inválida.");
        }

        if (horarios.length === 0) {
            mostrarFeedback(
                horariosFeedback,
                "Nenhum horário disponível. Escolha outra data ou profissional.",
                "info"
            );

            return;
        }

        horarios.forEach((horario) => {
            const opcao = document.createElement("option");

            // A API retorna TimeOnly, normalmente como "09:00:00".
            opcao.value = horario;
            opcao.textContent = horario.slice(0, 5);

            horarioSelect.appendChild(opcao);
        });

        horarioSelect.disabled = false;

        mostrarFeedback(
            horariosFeedback,
            `${horarios.length} horário(s) disponível(is). Selecione uma opção.`,
            "success"
        );
    } catch (erro) {
        if (numeroConsulta !== consultaHorariosAtual) {
            return;
        }

        mostrarFeedback(
            horariosFeedback,
            erro.message,
            "error"
        );
    }
}

async function consultarMeusAgendamentos(telefone) {
    limparFeedback(meusAgendamentosFeedback);
    meusAgendamentosLista.replaceChildren();

    try {
        const agendamentos =
            await buscarAgendamentosPorTelefone(telefone);

        renderizarMeusAgendamentos(agendamentos, telefone);

        if (agendamentos.length === 0) {
            mostrarFeedback(
                meusAgendamentosFeedback,
                "Nenhum agendamento encontrado para esse telefone.",
                "info"
            );
        } else {
            mostrarFeedback(
                meusAgendamentosFeedback,
                `${agendamentos.length} agendamento(s) encontrado(s).`,
                "success"
            );
        }
    } catch (erro) {
        mostrarFeedback(
            meusAgendamentosFeedback,
            erro.message,
            "error"
        );
    }
}
async function cancelarAgendamento(id, telefone, botao) {
    const confirmar = window.confirm(
        `Deseja realmente cancelar o agendamento #${id}?`
    );

    if (!confirmar) {
        return;
    }

    botao.disabled = true;
    botao.textContent = "Cancelando...";

    try {
        await apiRequest(`/api/agendamentos/${id}/cancelar`, {
            method: "POST",
            body: JSON.stringify({ telefone })
        });

        const agendamentos =
            await buscarAgendamentosPorTelefone(telefone);

        renderizarMeusAgendamentos(agendamentos, telefone);

        mostrarFeedback(
            meusAgendamentosFeedback,
            `Agendamento #${id} cancelado com sucesso.`,
            "success"
        );
    } catch (erro) {
        mostrarFeedback(
            meusAgendamentosFeedback,
            erro.message,
            "error"
        );

        botao.disabled = false;
        botao.textContent = "Cancelar agendamento";
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

    const data = dataInput.value;
    const horario = horarioSelect.value;

    if (!data || !horario || horarioSelect.disabled) {
        mostrarFeedback(
            agendamentoFeedback,
            "Selecione uma data e um horário disponível antes de continuar.",
            "error"
        );

        return;
    }

    configurarBotao(agendamentoForm, botaoTexto, true);

    // Combina data e horário sem acrescentar fuso horário.
    const dataHoraInicio = `${data}T${horario}`;

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
        ).toLocaleTimeString("pt-BR", {
            hour: "2-digit",
            minute: "2-digit"
        });

        mostrarFeedback(
            agendamentoFeedback,
            `Agendamento #${agendamento.id} confirmado para ${inicio}. Término previsto: ${fim}.`,
            "success"
        );

        // Mantém paciente, serviço e profissional selecionados,
        // mas limpa a data e os horários para a próxima reserva.
        dataInput.value = "";

        await carregarHorariosDisponiveis();
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

    // Converte o instante atual para a data local antes de formatar.
    agora.setMinutes(
        agora.getMinutes() - agora.getTimezoneOffset()
    );

    dataInput.min = agora.toISOString().slice(0, 10);
}
servicoSelect.addEventListener("change", async () => {
    const profissionalAnterior = profissionalSelect.value;

    try {
        await carregarProfissionais(
            servicoSelect.value,
            profissionalAnterior
        );
    } catch (erro) {
        mostrarFeedback(
            horariosFeedback,
            `Não foi possível carregar os profissionais: ${erro.message}`,
            "error"
        );

        return;
    }

    await carregarHorariosDisponiveis();
});

profissionalSelect.addEventListener(
    "change",
    carregarHorariosDisponiveis
);

dataInput.addEventListener(
    "change",
    carregarHorariosDisponiveis
);
async function buscarAgendamentosPorTelefone(telefone) {
    const parametros = new URLSearchParams({ telefone });

    return await apiRequest(
        `/api/agendamentos/meus?${parametros.toString()}`
    );
}

function renderizarMeusAgendamentos(agendamentos, telefone) {
    meusAgendamentosLista.replaceChildren();

    if (agendamentos.length === 0) {
        const mensagem = document.createElement("p");
        mensagem.className = "empty-message";
        mensagem.textContent =
            "Nenhum agendamento foi encontrado para esse telefone.";

        meusAgendamentosLista.appendChild(mensagem);
        return;
    }

    agendamentos.forEach((agendamento) => {
        const card = document.createElement("article");
        card.className = "appointment-card";

        const cabecalho = document.createElement("div");
        cabecalho.className = "appointment-card-header";

        const titulo = document.createElement("h3");
        titulo.textContent = agendamento.servico;

        const status = document.createElement("span");
        status.className = "appointment-status";

        if (agendamento.status === "Cancelado") {
            status.classList.add("appointment-status--cancelado");
        } else if (agendamento.status === "Concluido") {
            status.classList.add("appointment-status--concluido");
        }

        status.textContent = agendamento.status;

        cabecalho.append(titulo, status);

        const detalhes = document.createElement("div");
        detalhes.className = "appointment-details";

        const profissional = document.createElement("p");
        profissional.textContent =
            `Profissional: ${agendamento.profissional}`;

        const inicio = document.createElement("p");
        inicio.textContent =
            `Início: ${new Date(agendamento.dataHoraInicio)
                .toLocaleString("pt-BR", {
                    dateStyle: "short",
                    timeStyle: "short"
                })}`;

        const fim = document.createElement("p");
        fim.textContent =
            `Término: ${new Date(agendamento.dataHoraFim)
                .toLocaleTimeString("pt-BR", {
                    hour: "2-digit",
                    minute: "2-digit"
                })}`;

        const identificador = document.createElement("p");
        identificador.textContent = `Agendamento #${agendamento.id}`;

        detalhes.append(
            profissional,
            inicio,
            fim,
            identificador
        );

        card.append(cabecalho, detalhes);

        if (agendamento.status === "Agendado") {
            const botaoCancelar = document.createElement("button");
            botaoCancelar.type = "button";
            botaoCancelar.className = "button button-danger";
            botaoCancelar.textContent = "Cancelar agendamento";

            botaoCancelar.addEventListener("click", async () => {
                await cancelarAgendamento(
                    agendamento.id,
                    telefone,
                    botaoCancelar
                );
            });

            card.appendChild(botaoCancelar);
        }

        meusAgendamentosLista.appendChild(card);
    });
}
consultaAgendamentosForm.addEventListener(
    "submit",
    async (evento) => {
        evento.preventDefault();

        const telefone = consultaTelefoneInput.value.trim();

        if (!telefone) {
            mostrarFeedback(
                meusAgendamentosFeedback,
                "Informe o telefone utilizado no cadastro.",
                "error"
            );
            return;
        }

        const botao = consultaAgendamentosForm.querySelector(
            'button[type="submit"]'
        );

        botao.disabled = true;
        botao.textContent = "Consultando...";

        try {
            await consultarMeusAgendamentos(telefone);
        } finally {
            botao.disabled = false;
            botao.textContent = "Consultar agendamentos";
        }
    }
);


configurarDataMinima();
carregarDadosIniciais();