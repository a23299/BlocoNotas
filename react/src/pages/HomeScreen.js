// Importação dos módulos e dependências necessárias
import { signOut } from "firebase/auth";
import { database } from '../firebase';
import { useNavigate } from "react-router-dom";
import React, { useState, useEffect } from 'react';
import { Button, Modal, Form, Alert } from 'react-bootstrap';
import { v4 as uuidv4 } from 'uuid';
import { getAuth, onAuthStateChanged } from "firebase/auth";

function HomeScreen() {
    // Inicialização dos estados para armazenar as informações das notas e dos componentes do formulário
    const [notes, setNotes] = useState([]); // Armazena as notas recuperadas da API
    const [showAddModal, setShowAddModal] = useState(false); // Controla a exibição do modal de adição/edição de notas
    const [newTitulo, setNewTitulo] = useState(''); // Armazena o título da nova nota
    const [newConteudo, setNewConteudo] = useState(''); // Armazena o conteúdo da nova nota
    const [editingNoteId, setEditingNoteId] = useState(null); // Armazena o ID da nota sendo editada
    const [editedTitulo, setEditedTitulo] = useState(''); // Armazena o título da nota em edição
    const [editedConteudo, setEditedConteudo] = useState(''); // Armazena o conteúdo da nota em edição
    const [searchTerm, setSearchTerm] = useState(''); // Armazena o termo de busca para filtrar as notas
    const [searchResults, setSearchResults] = useState([]); // Armazena os resultados da busca
    const navigate = useNavigate(); // Função para navegar entre rotas
    const [userLoggedIn, setUserLoggedIn] = useState(false); // Verifica se o utilizador está logado

    // Recuperação das notas da API quando o componente é montado
    useEffect(() => {
        fetch('https://api.sheety.co/3c3661bd08795b26c99998297f39c730/blocoDeNotas/notas')
            .then(response => response.json())
            .then(data => {
                if (data && data.notas && Array.isArray(data.notas)) {
                    setNotes(data.notas); // Atualiza o estado com as notas recuperadas
                } else {
                    console.error('Dados recebidos não estão no formato esperado:', data);
                }
            })
            .catch(error => {
                console.error('Erro ao buscar notas:', error);
            });
    }, []);

    // Verificação do estado de autenticação do utilizador
    useEffect(() => {
        const auth = getAuth();
        const unsubscribe = onAuthStateChanged(auth, (user) => {
            if (user) {
                setUserLoggedIn(true); // Define como true se o utilizador estiver logado
            } else {
                setUserLoggedIn(false); // Define como false se o utilizador não estiver logado
            }
        });

        return () => unsubscribe(); // Limpa o evento de autenticação ao desmontar o componente
    }, []);

    // Função para lidar com o evento de login
    const handleLogin = () => {
        navigate('/'); // Navega para a rota de login
    };

    // Filtragem das notas com base no termo de busca
    useEffect(() => {
        const results = notes.filter(note =>
            note.titulo.toLowerCase().includes(searchTerm.toLowerCase())
        );
        setSearchResults(results); // Atualiza os resultados da busca com as notas filtradas
    }, [searchTerm, notes]);


    // Função para fechar o modal de adição/edição de notas
    const handleAddModalClose = () => {
        setShowAddModal(false);
        setNewTitulo('');
        setNewConteudo('');
        setEditingNoteId(null);
        setEditedTitulo('');
        setEditedConteudo('');
    };

    // Função para adicionar uma nova nota
    const handleAddNote = async () => {
        // Gera um novo ID para a nota usando a função uuidv4()
        const newId = uuidv4();

        // URL da API onde as notas são armazenadas
        const url = 'https://api.sheety.co/3c3661bd08795b26c99998297f39c730/blocoDeNotas/notas';

        // Corpo da requisição contendo os detalhes da nova nota
        const body = {
            nota: {
                id: newId, // Novo ID gerado
                titulo: newTitulo, // Título da nova nota
                conteudo: newConteudo, // Conteúdo da nova nota
            }
        };

        try {
            // Realiza uma requisição POST para adicionar a nova nota à API
            const response = await fetch(url, {
                method: 'POST', // Método HTTP: POST para adicionar novo registro
                headers: {
                    'Content-Type': 'application/json', // Indica que o corpo da requisição é um JSON
                },
                body: JSON.stringify(body), // Converte o objeto 'body' em formato JSON
            });

            // Verifica se a resposta da requisição foi bem-sucedida (código 200 a 299)
            if (response.ok) {
                // Extrai os dados JSON da resposta
                const data = await response.json();
                // Obtém a nova nota adicionada a partir dos dados da resposta
                const addedNote = data.nota;
                // Adiciona a nova nota ao estado 'notes' (lista de notas) utilizando o spread operator
                setNotes([...notes, addedNote]);
                // Fecha o modal de adição/edição de notas após adicionar a nova nota
                handleAddModalClose();
            } else {
                // Se a resposta não foi ok, exibe um erro no console com o código de status da resposta
                console.error('Erro ao adicionar nota:', response.status);
            }
        } catch (error) {
            // Se ocorrer um erro durante a requisição, exibe o erro no console
            console.error('Erro ao adicionar nota:', error);
        }
    };


// Função para apagar uma nota existente
const handleDeleteNote = async (noteId) => {
    // URL específica para apagar a nota com base no 'noteId' fornecido
    const url = `https://api.sheety.co/3c3661bd08795b26c99998297f39c730/blocoDeNotas/notas/${noteId}`;

    try {
        // Realiza uma requisição DELETE para remover a nota da API
        const response = await fetch(url, {
            method: 'DELETE', // Método HTTP: DELETE para remover um registro existente
        });

        // Verifica se a resposta da requisição foi bem-sucedida 
        if (response.ok) {
            // Filtra a lista de notas para remover a nota apagada com base no 'noteId'
            const updatedNotes = notes.filter(note => note.id !== noteId);
            // Atualiza o estado 'notes' (lista de notas) removendo a nota apagada
            setNotes(updatedNotes);
        } else {
            // Se a resposta não foi ok, exibe um erro na consola com o código de status da resposta
            console.error('Erro ao apagar nota:', response.status);
        }
    } catch (error) {
        // Se ocorrer um erro durante a requisição, exibe o erro na consola
        console.error('Erro ao apagar nota:', error);
    }
};

// Função para salvar uma nota editada
const handleSaveEditedNote = async () => {
    // Encontra o índice da nota a ser editada com base no 'editingNoteId'
    const editedNoteIndex = notes.findIndex(note => note.id === editingNoteId);
    // Cria um objeto com as informações editadas da nota
    const editedNote = {
        id: editingNoteId,
        titulo: editedTitulo,
        conteudo: editedConteudo,
    };

    // Cria uma cópia atualizada da lista de notas
    const updatedNotes = [...notes];
    // Substitui a nota antiga pela nota editada na cópia das notas atualizadas
    updatedNotes[editedNoteIndex] = editedNote;
    // Atualiza o estado 'notes' com a lista de notas atualizada
    setNotes(updatedNotes);
    // Fecha o modal de edição/adicionar nota
    handleAddModalClose();

    // URL específica para atualizar a nota na API com base no 'editingNoteId'
    const url = `https://api.sheety.co/3c3661bd08795b26c99998297f39c730/blocoDeNotas/notas/${editingNoteId}`;
    // Corpo da requisição com os dados atualizados da nota
    const body = {
        nota: {
            titulo: editedTitulo,
            conteudo: editedConteudo,
        }
    };

    try {
        // Realiza uma requisição PUT para atualizar a nota na API
        const response = await fetch(url, {
            method: 'PUT', // Método HTTP: PUT para atualizar um registo existente
            headers: {
                'Content-Type': 'application/json', // Tipo de conteúdo JSON na requisição
            },
            body: JSON.stringify(body), // Converte o corpo da requisição para formato JSON
        });

        // Verifica se a resposta da requisição não foi bem-sucedida 
        if (!response.ok) {
            console.error('Erro ao editar nota na API:', response.status);
        }
    } catch (error) {
        // Se ocorrer um erro durante a requisição, exibe o erro na consola
        console.error('Erro ao editar nota na API:', error);
    }
};

    // Função para lidar com o evento de logout
    const handleClick = () => {
        signOut(database).then(val => {
            console.log(val, "val")
            navigate('/')
        })
    };

    // Função para editar uma nota existente
    const handleEditNote = (noteId) => {
        const noteToEdit = notes.find(note => note.id === noteId);
        setEditingNoteId(noteId);
        setEditedTitulo(noteToEdit.titulo);
        setEditedConteudo(noteToEdit.conteudo);
        setShowAddModal(true);
    };

    // Renderização condicional com base no estado do utilizador logado
    return (
        <div className="container mt-4">
            {userLoggedIn ? (
                // Se o utilizador estiver logado
                <div>
                    <h1>Notas</h1>
                    <input
                        type="text"
                        placeholder="Pesquisar por título"
                        value={searchTerm}
                        onChange={(e) => {
                            setSearchTerm(e.target.value);
                        }}
                    />
                    <ul className="list-group mt-3">
                        {/* Mapeamento dos resultados da busca para exibição das notas */}
                        {searchResults.map(note => (
                            <li
                                key={note.id}
                                className="list-group-item d-flex justify-content-between align-items-center"
                            >
                                <div>
                                    <h4>{note.titulo}</h4>
                                    <p>{note.conteudo}</p>
                                </div>
                                <div>
                                    {/* Botões para editar e apagar notas */}
                                    <Button
                                        variant="primary"
                                        size="sm"
                                        onClick={() => handleEditNote(note.id)}
                                    >
                                        Editar
                                    </Button>
                                    <Button
                                        variant="danger"
                                        size="sm"
                                        onClick={() => handleDeleteNote(note.id)}
                                    >
                                        Apagar
                                    </Button>
                                </div>
                            </li>
                        ))}
                    </ul>
                    {/* Botão para adicionar uma nova nota */}
                    <Button variant="success" onClick={() => setShowAddModal(true)}>
                        Adicionar Nota
                    </Button>
                    {/* Modal para adição/edição de notas */}
                    <Modal show={showAddModal} onHide={handleAddModalClose}>
                        <Modal.Header closeButton>
                            <Modal.Title>{editingNoteId !== null ? 'Editar Nota' : 'Adicionar Nota'}</Modal.Title>
                        </Modal.Header>
                        <Modal.Body>
                            <Form>
                                <Form.Group controlId="noteTitle">
                                    <Form.Label>Título</Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Título da nota"
                                        value={editingNoteId !== null ? editedTitulo : newTitulo}
                                        onChange={(e) => {
                                            if (editingNoteId !== null) {
                                                setEditedTitulo(e.target.value);
                                            } else {
                                                setNewTitulo(e.target.value);
                                            }
                                        }}
                                    />
                                </Form.Group>
                                <Form.Group controlId="noteContent">
                                    <Form.Label>Conteúdo</Form.Label>
                                    <Form.Control
                                        as="textarea"
                                        rows={3}
                                        placeholder="Conteúdo da nota"
                                        value={editingNoteId !== null ? editedConteudo : newConteudo}
                                        onChange={(e) => {
                                            if (editingNoteId !== null) {
                                                setEditedConteudo(e.target.value);
                                            } else {
                                                setNewConteudo(e.target.value);
                                            }
                                        }}
                                    />
                                </Form.Group>
                                {/* Botão para salvar a nota editada ou adicionar uma nova nota */}
                                <Button variant="primary" onClick={editingNoteId !== null ? handleSaveEditedNote : handleAddNote}>
                                    {editingNoteId !== null ? 'Salvar Nota Editada' : 'Adicionar Nota'}
                                </Button>
                            </Form>
                        </Modal.Body>
                    </Modal>
                    {/* Botão para realizar logout */}
                    <div>
                        <Button variant="primary" onClick={handleClick}>
                            Sign Out
                        </Button>
                    </div>
                </div>
            ) : (
                // Se o utilizador não estiver logado, exibe uma mensagem de alerta e um botão para login
                <Alert variant="warning">
                    Você precisa fazer login para aceder a esta página.
                    <Button variant="primary" onClick={handleLogin}>
                        Fazer Login
                    </Button>
                </Alert>
            )}
        </div>
    );
}

export default HomeScreen;