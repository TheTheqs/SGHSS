# SGHSS – Sistema de Gestão Hospitalar com Suporte a Serviços de Saúde

Sistema de Gestão Hospitalar desenvolvido em **ASP.NET Core** como trabalho de conclusão de curso (TCC).  
O objetivo do SGHSS é oferecer uma **API REST** para gestão de unidades de saúde, leitos, pacientes, profissionais, internações, agendamentos, prescrições eletrônicas, notificações e auditoria de atividades.

---

## Sumário

- [Objetivos do Sistema](#objetivos-do-sistema)
- [Principais Funcionalidades](#principais-funcionalidades)
  - [Administração e Governança](#administração-e-governança)
  - [Pacientes](#pacientes)
  - [Profissionais de Saúde](#profissionais-de-saúde)
  - [Agendamentos e Teleconsulta](#agendamentos-e-teleconsulta)
  - [Internações e Leitos](#internaçãos-e-leitos)
  - [Home Care](#home-care)
  - [Notificações e Auditoria](#notificações-e-auditoria)
- [Arquitetura da Solução](#arquitetura-da-solução)
  - [Camada Domain](#camada-domain)
  - [Camada Application](#camada-application)
  - [Camada Infra](#camada-infra)
  - [Camada Interface (API)](#camada-interface-api)
- [Segurança, Autenticação e Autorização](#segurança-autenticação-e-autorização)
- [Persistência e Acesso a Dados](#persistência-e-acesso-a-dados)
- [Execução do Projeto](#execução-do-projeto)
  - [Pré-requisitos](#pré-requisitos)
  - [Configuração de Ambiente](#configuração-de-ambiente)
  - [Rodando a Aplicação](#rodando-a-aplicação)
- [Padronização de DTOs e Mapeamentos](#padronização-de-dtos-e-mapeamentos)
- [Principais Casos de Uso](#principais-casos-de-uso)
  - [Administradores](#casos-de-uso---administradores)
  - [Gestão de Leitos e Internações](#casos-de-uso---gestão-de-leitos-e-internações)
  - [Agendamentos e Agenda Profissional](#casos-de-uso---agendamentos-e-agenda-profissional)
- [Documentação da API](#documentação-da-api)
- [Limitações e Trabalhos Futuros](#limitações-e-trabalhos-futuros)
- [Licença](#licença)

---

## Objetivos do Sistema

- **Centralizar** a gestão de informações de pacientes, profissionais, unidades de saúde e leitos.
- **Apoiar a operação hospitalar** com:
  - controle de internações,
  - emissão de prescrições eletrônicas,
  - registros de home care,
  - emissão de atestados médicos digitais.
- **Garantir rastreabilidade** das ações através de logs e relatórios de auditoria.
- Servir como **prova de conceito acadêmica**, demonstrando:
  - boas práticas de arquitetura (camadas, use cases, DTOs, value objects),
  - uso de **Entity Framework Core**,
  - autenticação JWT e controle de acesso,
  - documentação automática de API.

---

## Principais Funcionalidades

### Administração e Governança

- Cadastro e gestão de **Administradores**:
  - Verificação de existência por e-mail.
  - Registro de novos administradores.
  - Listagem de administradores em formato simplificado (ID e Nome).
- Cadastro e gestão de **Unidades de Saúde (HealthUnit)**:
  - Verificação de CNPJ já cadastrado.
  - Registro de novas unidades com:
    - nome,
    - CNPJ,
    - telefone institucional,
    - endereço (value object),
    - tipo da unidade.
  - Consulta de todas as unidades ativas.

- **Inicialização do sistema**:
  - Caso de uso `EnsureDefaultSuperAdministratorUseCase`:
    - Garante a existência de um administrador **SUPER** padrão para ambiente acadêmico/de teste.
    - As credenciais são fixas apenas para o contexto do TCC e **não devem ser usadas em produção**.

### Pacientes

- Cadastro de pacientes com:
  - dados pessoais,
  - CPF (value object),
  - e-mail (value object).
- Consulta de paciente por ID, incluindo relacionamentos:
  - internações,
  - prontuário médico,
  - outros agregados relevantes.
- Atualização de dados do paciente, incluindo agregados.
- Consulta de todos os pacientes.

### Profissionais de Saúde

- Cadastro de **Profissionais** com:
  - registro profissional (`ProfessionalLicense`),
  - verificação de duplicidade de licença profissional,
  - verificação de duplicidade de e-mail.
- Atualização de profissionais e seus agregados (agenda, política de agenda, etc.).
- Listagem de todos os profissionais cadastrados.

### Agendamentos e Teleconsulta

- Registro e atualização de **consultas (Appointments)**.
- Consulta de agendamento por ID.
- Consulta de todas as consultas de um paciente.
- Consulta do **link de teleconsulta** associado a um agendamento:
  - Caso de uso `GetAppointmentLinkUseCase`:
    - Recebe o `AppointmentId`.
    - Retorna a URL de teleconsulta configurada para aquele agendamento.

### Internações e Leitos

- **Gestão de Leitos (Beds)**:
  - Leitos associados a uma unidade de saúde.
  - Estados de leito:
    - Disponível,
    - Ocupado,
    - Em manutenção, etc.
  - Filtros:
    - por tipo de leito,
    - por status.

- **Consulta de leitos por unidade**:
  - `ConsultHealthUnitBedsUseCase`:
    - Permite filtrar por tipo e status.
    - Retorna as camas em formato `BedDto`.

- **Hospitalização de Paciente**:
  - `HospitalizePatientUseCase`:
    - Garante que:
      - a cama existe e está disponível;
      - o paciente não possua outra internação ativa;
    - Cria uma nova `Hospitalization` com status **Admitted**;
    - Atualiza o status da cama para **Occupied**;
    - Retorna dados da internação (HospitalizationId, PatientId, BedId, AdmissionDate, Reason).

- **Alta de Paciente**:
  - `DischargePatientUseCase`:
    - Localiza a internação ativa do paciente;
    - Registra a data de alta (UTC atual);
    - Atualiza o status da internação para **Discharged**;
    - Libera a cama ligada à internação (status **Available**).

- **Manutenção de camas**:
  - `MakeBedAsUnderMaintenanceUseCase`:
    - Coloca uma cama disponível em estado **UnderMaintenance**.
  - `MakeBedAsAvailableUseCase`:
    - Move cama de **UnderMaintenance** para **Available**.
  - Validações garantem consistência do fluxo de estados.

- **Gerenciamento de capacidade de leitos**:
  - `ManageBedsUseCase`:
    - Permite **adicionar** ou **remover** leitos de uma unidade.
    - Usa `ManageBedsRequest` com:
      - HealthUnitId,
      - modelo do leito (`BedDto`),
      - quantidade,
      - flag `IsAdding` para indicar adição ou remoção.
    - Retorna `ManageBedsResponse` com HealthUnitId e lista atualizada de leitos.

### Home Care

- Registro e atualização de **Home Care**:
  - Armazena atendimentos domiciliares associados a paciente.
- Consulta por:
  - ID do home care,
  - ID do paciente.

### Notificações e Auditoria

- **Notificações (Notification)**:
  - Persistência de notificações.
  - Atualização de notificações (por exemplo, leitura).
  - Consulta por:
    - ID da notificação,
    - ID do usuário destinatário.

- **Logs de Atividade (LogActivity)**:
  - Registro de ações relevantes do sistema para auditoria.
  - Consulta de logs por intervalo de tempo (`from` / `to`).

- **Relatórios de Auditoria (AuditReport)**:
  - Criação e persistência de relatórios de auditoria.
  - Consulta por ID de administrador:
    - resultados ordenados por data de criação decrescente.

---

## Arquitetura da Solução

O projeto segue uma organização em **camadas** inspirada em Clean Architecture / DDD, com forte uso de **Use Cases** e **Value Objects**.

### Camada Domain

Contém as **entidades de negócio** e **Value Objects**, como:

- Entidades:
  - `Administrator`, `Patient`, `Professional`, `User`
  - `HealthUnit`, `Bed`, `Hospitalization`
  - `Appointment`, `HomeCare`
  - `InventoryItem`
  - `Notification`
  - `DigitalMedicalCertificate`
  - `EletronicPrescription`
  - `AuditReport`, `LogActivity`
  - `ProfessionalSchedule`, `SchedulePolicy`, `ScheduleSlot`
- Value Objects:
  - `Email`, `Cpf`, `Cnpj`, `Address`, `ProfessionalLicense`
  - `WeeklyWindow`, entre outros.
- Enums:
  - `AccessLevel`, `BedStatus`, `HospitalizationStatus`, `ConsentScope`, etc.

A camada Domain **não depende** de nenhuma tecnologia de infraestrutura.

### Camada Application

Responsável por:

- **Casos de uso (Use Cases)**:
  - Cada cenário de negócio possui uma classe própria, ex:
    - `RegisterAdministratorUseCase`,
    - `RegisterHealthUnitUseCase`,
    - `HospitalizePatientUseCase`,
    - `DischargePatientUseCase`,
    - `ManageBedsUseCase`,
    - `GetAppointmentLinkUseCase`,
    - etc.
- **DTOs**:
  - Requests e Responses específicos de cada caso de uso.
- **Interfaces de repositório**:
  - `IAdministratorRepository`, `IPatientRepository`, `IProfessionalRepository`, etc.
- **Serviços de aplicação**:
  - Ex.: `ITokenService` para geração de tokens de autenticação.
- **Mappers**:
  - Ex.: `AddressMapper`, `BedMapper`, `ScheduleSlotMapper`, `WeeklyWindowMapper`, `SchedulePolicyMapper`.

### Camada Infra

Responsável pela **implementação concreta** das interfaces da camada Application, incluindo:

- Repositórios com **Entity Framework Core**.
- Mapeamentos de entidades para tabelas (via Fluent API / `IEntityTypeConfiguration`).
- Configuração de banco de dados.

### Camada Interface (API)

- Controladores ASP.NET Core (`Controllers`) expondo endpoints REST, por exemplo:
  - Controladores para Administradores, Pacientes, Profissionais, Unidades de Saúde, Agendamentos, Internações, Prescrições, Notificações, etc.
- Roteamento padrão:
  - `[Route("api/[controller]")]`.
- Integração com:
  - **Autenticação JWT**,
  - **Swagger/OpenAPI** para documentação.

---

## Segurança, Autenticação e Autorização

- Uso de **JWT** para autenticação.
- Serviço `ITokenService`:
  - Gera tokens para instâncias de `User` com o `AccessLevel` correspondente.
  - Devolve o token e a data/hora de expiração.
- O **nível de acesso** (Role) é baseado no enum `AccessLevel`, distinguindo perfis como:
  - Paciente,
  - Profissional,
  - Administrador (incluindo SUPER).
- As credenciais do administrador SUPER padrão são configuradas para **uso acadêmico** e devem ser movidas para variáveis de ambiente / cofre de segredos em cenário real.

---

## Persistência e Acesso a Dados

- Acesso a dados abstraído por interfaces de repositório na camada Application, como:
  - `IPatientRepository`, `IProfessionalRepository`, `IAppointmentRepository`,
  - `IHealthUnitRepository`, `IBedRepository`, `IHospitalizationRepository`,
  - `IInventoryItemRepository`, `INotificationRepository`,
  - `ILogActivityRepository`, `IAuditReportRepository`, etc.
- Operações assíncronas (`Async`) para:
  - Inclusão (`AddAsync`),
  - Atualização (`UpdateAsync`),
  - Consulta por ID,
  - Consultas específicas (por paciente, por período, por unidade, etc.).
- As implementações concretas usam **Entity Framework Core** para mapear as entidades para tabelas em um banco relacional.

---

## Execução do Projeto

> **Obs.:** Ajuste o nome das soluções/projetos e strings de conexão conforme sua estrutura real.

### Pré-requisitos

- **.NET SDK 10.0** (ou versão correspondente ao `TargetFramework` do projeto).
- Banco de dados relacional compatível com o provedor configurado (por ex. SQL Server / PostgreSQL).
- Ferramentas recomendadas:
  - Visual Studio / Rider / VS Code + extensão C#,
  - `dotnet-ef` global tool para migrações EF Core.

### Configuração de Ambiente

1. **Clonar o repositório**

   ```bash
   git clone <https://github.com/TheTheqs/SGHSS>
   cd SGHSS
