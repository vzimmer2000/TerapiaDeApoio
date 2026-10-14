const agendaDataInput = document.getElementById("agenda-data");
const agendaProfissionalSelect =
    document.getElementById("agenda-profissional");

const agendaForm = document.getElementById("form-agenda");
const agendaFeedback = document.getElementById("agenda-feedback");
const agendaTbody = document.getElementById("agenda-tbody");
const agendaDataTitulo = document.getElementById("agenda-data-titulo");

let agendaAtual = [];

// Comunicação HTTP com a API.

async function requisicaoApi(endpoint, options = {}) {
    const headers = { ...(options.headers ?? {}) };

    if (options.body) {
        headers["Content-Type"] = "application/json; charset=utf-8";
    }

    const response = await fetch(endpoint, {
        ...options,
        headers
    });

    if (response.status === 204) {
        return null;
    }

    const texto = await response.text();
    let dados = null;

    if (texto) {
        try {
            dados = JSON.parse(texto);
        } catch {
            dados = { detail: texto };
        }
    }

    if (!response.ok) {
        const errosValidacao = dados?.errors
            ? Object.values(dados.errors).flat().join(" ")
            : "";

        throw new Error(
            dados?.detail ||
            errosValidacao ||
            dados?.title ||
            `Erro HTTP ${response.status}.`
        );
    }

    return dados;
}

function mostrarMensagem(mensagem, tipo = "info") {
    agendaFeedback.textContent = mensagem;
    agendaFeedback.className = `admin-feedback admin-feedback--${tipo}`;
}

function limparMensagem() {
    agendaFeedback.textContent = "";
    agendaFeedback.className = "admin-feedback";
}

// Manipulação de datas locais.

function obterDataLocal(date = new Date()) {
    const ano = date.getFullYear();
    const mes = String(date.getMonth() + 1).padStart(2, "0");
    const dia = String(date.getDate()).padStart(2, "0");

    return `${ano}-${mes}-${dia}`;
}

function deslocarData(valor, quantidadeDias) {
    const [ano, mes, dia] = valor.split("-").map(Number);
    const data = new Date(ano, mes - 1, dia);
    data.setDate(data.getDate() + quantidadeDias);

    return obterDataLocal(data);
}

function formatarData(valor) {
    const [ano, mes, dia] = valor.split("-").map(Number);

    return new Date(ano, mes - 1, dia).toLocaleDateString("pt-BR", {
        weekday: "long",
        day: "2-digit",
        month: "long",
        year: "numeric"
    });
}

function formatarHora(valor) {
    const data = new Date(valor);

    if (Number.isNaN(data.getTime())) {
        return String(valor).slice(11, 16);
    }

    return data.toLocaleTimeString("pt-BR", {
        hour: "2-digit",
        minute: "2-digit"
    });
}

// Carregamento dos profissionais e resumo dos cadastros.

async function carregarProfissionais() {
    const profissionais = await requisicaoApi("/api/profissionais");

    agendaProfissionalSelect.replaceChildren();

    const todos = document.createElement("option");
    todos.value = "";
    todos.textContent = "Todos os profissionais";
    agendaProfissionalSelect.appendChild(todos);

    for (const profissional of profissionais) {
        const opcao = document.createElement("option");
        opcao.value = String(profissional.id);

        opcao.textContent = profissional.ativo
            ? `${profissional.nome} — ${profissional.especialidade}`
            : `${profissional.nome} — inativo`;

        agendaProfissionalSelect.appendChild(opcao);
    }
}



function renderizarTabelaVazia(tbody, colunas, mensagem) {
    const linha = document.createElement("tr");
    const celula = document.createElement("td");

    celula.colSpan = colunas;
    celula.className = "table-message";
    celula.textContent = mensagem;

    linha.appendChild(celula);
    tbody.replaceChildren(linha);
}

function renderizarTabelaPacientes(pacientes) {
    const tbody = document.getElementById("admin-pacientes-tbody");
    tbody.replaceChildren();

    if (pacientes.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum paciente cadastrado."
        );
        return;
    }

    pacientes.forEach((paciente) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(paciente.nome));
        linha.appendChild(criarCelula(paciente.telefone));
        linha.appendChild(criarCelula(paciente.email || "—"));

        tbody.appendChild(linha);
    });
}

function renderizarTabelaProfissionais(profissionais) {
    const tbody = document.getElementById("admin-profissionais-tbody");
    tbody.replaceChildren();

    if (profissionais.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum profissional cadastrado."
        );
        return;
    }

    profissionais.forEach((profissional) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(profissional.nome));
        linha.appendChild(criarCelula(profissional.especialidade));
        linha.appendChild(
            criarCelula(profissional.ativo ? "Ativo" : "Inativo")
        );

        tbody.appendChild(linha);
    });
}

function renderizarTabelaServicos(servicos) {
    const tbody = document.getElementById("admin-servicos-tbody");
    tbody.replaceChildren();

    if (servicos.length === 0) {
        renderizarTabelaVazia(
            tbody,
            3,
            "Nenhum serviço ativo cadastrado."
        );
        return;
    }

    servicos.forEach((servico) => {
        const linha = document.createElement("tr");

        linha.appendChild(criarCelula(servico.nome));
        linha.appendChild(criarCelula(servico.descricao || "—"));
        linha.appendChild(
            criarCelula(`${servico.duracaoMinutos} minutos`)
        );

        tbody.appendChild(linha);
    });
}

async function carregarResumoCadastros() {
    const feedback = document.getElementById("admin-listas-feedback");

    try {
        const [pacientes, profissionais, servicos] = await Promise.all([
            requisicaoApi("/api/pacientes"),
            requisicaoApi("/api/profissionais"),
            requisicaoApi("/api/servicos")
        ]);

        document.getElementById("resumo-pacientes").textContent =
            pacientes.length;

        document.getElementById("resumo-profissionais").textContent =
            profissionais.length;

        document.getElementById("resumo-servicos").textContent =
            servicos.length;

        renderizarTabelaPacientes(pacientes);
        renderizarTabelaProfissionais(profissionais);
        renderizarTabelaServicos(servicos);

        if (feedback) {
            feedback.textContent = "";
            feedback.className = "admin-feedback";
        }
    } catch (erro) {
        console.error("Falha ao carregar os cadastros:", erro);

        if (feedback) {
            feedback.textContent =
                `Não foi possível atualizar os cadastros: ${erro.message}`;

            feedback.className = "admin-feedback admin-feedback--error";
        }
    }
}


// Resumo numérico da agenda consultada.

function atualizarIndicadores(agendamentos) {
    const totalAgendados = agendamentos.filter(
        (a) => a.status === "Agendado"
    ).length;

    const totalConcluidos = agendamentos.filter(
        (a) => a.status === "Concluido"
    ).length;

    const totalCancelados = agendamentos.filter(
        (a) => a.status === "Cancelado"
    ).length;

    document.getElementById("total-atendimentos").textContent =
        agendamentos.length;

    document.getElementById("total-agendados").textContent =
        totalAgendados;

    document.getElementById("total-concluidos").textContent =
        totalConcluidos;

    document.getElementById("total-cancelados").textContent =
        totalCancelados;
}

// Renderização segura da tabela.

function criarCelula(texto, classe = "") {
    const celula = document.createElement("td");
    celula.textContent = texto;

    if (classe) {
        celula.className = classe;
    }

    return celula;
}

function renderizarAgenda(agendamentos) {
    agendaTbody.replaceChildren();

    if (agendamentos.length === 0) {
        const linha = document.createElement("tr");
        const celula = document.createElement("td");

        celula.colSpan = 7;
        celula.className = "table-message";
        celula.textContent = "Nenhum atendimento encontrado para esta data.";

        linha.appendChild(celula);
        agendaTbody.appendChild(linha);

        atualizarIndicadores([]);
        return;
    }

    agendamentos.forEach((agendamento) => {
        const linha = document.createElement("tr");

        linha.appendChild(
            criarCelula(
                `${formatarHora(agendamento.inicio)}–${formatarHora(agendamento.fim)}`,
                "appointment-time"
            )
        );

        linha.appendChild(
            criarCelula(agendamento.paciente, "appointment-patient")
        );

        linha.appendChild(
            criarCelula(agendamento.telefone, "appointment-phone")
        );

        linha.appendChild(criarCelula(agendamento.servico));
        linha.appendChild(criarCelula(agendamento.profissional));

        const celulaStatus = document.createElement("td");
        const status = document.createElement("span");
        const classeStatus = String(agendamento.status).toLowerCase();

        status.className = `status-label status-label--${classeStatus}`;
        status.textContent = agendamento.status;

        celulaStatus.appendChild(status);
        linha.appendChild(celulaStatus);

        const celulaAcoes = document.createElement("td");

        if (agendamento.status === "Agendado") {
            const botao = document.createElement("button");

            botao.type = "button";
            botao.className = "button button-danger button-small";
            botao.textContent = "Cancelar";

            botao.addEventListener("click", () => {
                cancelarAgendamento(agendamento, botao);
            });

            celulaAcoes.appendChild(botao);
        } else {
            celulaAcoes.textContent = "—";
        }

        linha.appendChild(celulaAcoes);
        agendaTbody.appendChild(linha);
    });

    atualizarIndicadores(agendamentos);
}

// Consulta da agenda diária.

async function carregarAgenda() {
    const data = agendaDataInput.value;

    if (!data) {
        mostrarMensagem("Selecione uma data para consultar a agenda.", "error");
        return;
    }

    limparMensagem();
    mostrarMensagem("Carregando atendimentos...", "info");

    agendaTbody.replaceChildren();

    const parametros = new URLSearchParams({ data });

    if (agendaProfissionalSelect.value) {
        parametros.set(
            "profissionalId",
            agendaProfissionalSelect.value
        );
    }

    try {
        const agendamentos = await requisicaoApi(
            `/api/agendamentos/agenda?${parametros.toString()}`
        );

        agendaAtual = agendamentos;
        agendaDataTitulo.textContent = formatarData(data);

        renderizarAgenda(agendaAtual);

        mostrarMensagem(
            `${agendaAtual.length} atendimento(s) encontrado(s).`,
            "success"
        );
    } catch (erro) {
        agendaAtual = [];
        renderizarAgenda([]);

        mostrarMensagem(
            `Não foi possível carregar a agenda: ${erro.message}`,
            "error"
        );
    }
}

// Cancelamento pela interface administrativa.

async function cancelarAgendamento(agendamento, botao) {
    const confirmado = window.confirm(
        `Deseja cancelar o agendamento #${agendamento.id} de ${agendamento.paciente}?`
    );

    if (!confirmado) {
        return;
    }

    botao.disabled = true;
    botao.textContent = "Cancelando...";

    try {
        await requisicaoApi(
            `/api/agendamentos/${agendamento.id}/cancelar`,
            {
                method: "POST",
                body: JSON.stringify({
                    telefone: agendamento.telefone
                })
            }
        );

        await carregarAgenda();

        mostrarMensagem(
            `Agendamento #${agendamento.id} cancelado.`,
            "success"
        );
    } catch (erro) {
        mostrarMensagem(
            `Não foi possível cancelar: ${erro.message}`,
            "error"
        );

        botao.disabled = false;
        botao.textContent = "Cancelar";
    }
}

// Eventos da interface.

agendaForm.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    await carregarAgenda();
});

document.getElementById("agenda-hoje").addEventListener("click", async () => {
    agendaDataInput.value = obterDataLocal();
    await carregarAgenda();
});

document.getElementById("agenda-anterior").addEventListener(
    "click",
    async () => {
        if (!agendaDataInput.value) {
            agendaDataInput.value = obterDataLocal();
        }

        agendaDataInput.value = deslocarData(
            agendaDataInput.value,
            -1
        );

        await carregarAgenda();
    }
);

document.getElementById("agenda-proximo").addEventListener(
    "click",
    async () => {
        if (!agendaDataInput.value) {
            agendaDataInput.value = obterDataLocal();
        }

        agendaDataInput.value = deslocarData(
            agendaDataInput.value,
            1
        );

        await carregarAgenda();
    }
);

function mostrarFeedbackCadastro(id, mensagem, tipo = "success") {
    const elemento = document.getElementById(id);

    elemento.textContent = mensagem;
    elemento.className = `feedback feedback--${tipo}`;
}

async function executarCadastro({
    formId,
    feedbackId,
    endpoint,
    obterDados,
    mensagemSucesso,
    atualizarProfissionais = false
}) {
    const formulario = document.getElementById(formId);
    const botao = formulario.querySelector('button[type="submit"]');
    const textoOriginal = botao.textContent;

    botao.disabled = true;
    botao.textContent = "Salvando...";

    try {
        await requisicaoApi(endpoint, {
            method: "POST",
            body: JSON.stringify(obterDados(formulario))
        });

        formulario.reset();

        mostrarFeedbackCadastro(
            feedbackId,
            mensagemSucesso,
            "success"
        );

        await carregarResumoCadastros();

        if (atualizarProfissionais) {
            await carregarProfissionais();
        }
    } catch (erro) {
        mostrarFeedbackCadastro(
            feedbackId,
            erro.message,
            "error"
        );
    } finally {
        botao.disabled = false;
        botao.textContent = textoOriginal;
    }
}

document.getElementById("admin-form-paciente")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-paciente",
            feedbackId: "admin-paciente-feedback",
            endpoint: "/api/pacientes",
            obterDados: () => ({
                nome: document.getElementById("admin-paciente-nome").value.trim(),
                telefone: document.getElementById("admin-paciente-telefone").value.trim(),
                email: document.getElementById("admin-paciente-email").value.trim() || null
            }),
            mensagemSucesso: "Paciente cadastrado com sucesso."
        });
    });

document.getElementById("admin-form-profissional")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-profissional",
            feedbackId: "admin-profissional-feedback",
            endpoint: "/api/profissionais",
            obterDados: () => ({
                nome: document.getElementById("admin-profissional-nome").value.trim(),
                especialidade: document.getElementById("admin-profissional-especialidade").value.trim()
            }),
            mensagemSucesso: "Profissional cadastrado com sucesso.",
            atualizarProfissionais: true
        });
    });

document.getElementById("admin-form-servico")
    .addEventListener("submit", async (evento) => {
        evento.preventDefault();

        await executarCadastro({
            formId: "admin-form-servico",
            feedbackId: "admin-servico-feedback",
            endpoint: "/api/servicos",
            obterDados: () => ({
                nome: document.getElementById("admin-servico-nome").value.trim(),
                descricao: document.getElementById("admin-servico-descricao").value.trim() || null,
                duracaoMinutos: Number(document.getElementById("admin-servico-duracao").value)
            }),
            mensagemSucesso: "Serviço cadastrado com sucesso."
        });
    });

document.getElementById("admin-atualizar-cadastros")
    .addEventListener("click", async () => {
        await carregarResumoCadastros();
        await carregarProfissionais();
    });


async function inicializarPainel() {
    agendaDataInput.value = obterDataLocal();

    try {
        await carregarProfissionais();
    } catch (erro) {
        mostrarMensagem(
            `Não foi possível carregar os profissionais: ${erro.message}`,
            "error"
        );
    }

    await Promise.all([
        carregarResumoCadastros(),
        carregarAgenda()
    ]);
}

inicializarPainel();