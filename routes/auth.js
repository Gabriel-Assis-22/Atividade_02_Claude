'use strict';

const express = require('express');
const bcrypt = require('bcryptjs');
const db = require('../db');

const router = express.Router();

// GET /auth/login
router.get('/login', (req, res) => {
  if (req.session.userId) return res.redirect('/catalog');
  res.render('login', { erro: null });
});

// POST /auth/login
router.post('/login', async (req, res) => {
  const { email, senha } = req.body;

  if (!email || !senha) {
    return res.render('login', { erro: 'Preencha todos os campos.' });
  }

  try {
    const [rows] = await db.query(
      'SELECT * FROM usuarios WHERE email = ?',
      [email]
    );

    if (rows.length === 0) {
      return res.render('login', { erro: 'E-mail ou senha inválidos.' });
    }

    const usuario = rows[0];
    const senhaValida = await bcrypt.compare(senha, usuario.senha_hash);

    if (!senhaValida) {
      return res.render('login', { erro: 'E-mail ou senha inválidos.' });
    }

    req.session.userId = usuario.id;
    req.session.usuario = { id: usuario.id, nome: usuario.nome, email: usuario.email };

    res.redirect('/catalog');
  } catch (err) {
    console.error('Erro no login:', err);
    res.render('login', { erro: 'Erro interno. Tente novamente.' });
  }
});

// GET /auth/register
router.get('/register', (req, res) => {
  if (req.session.userId) return res.redirect('/catalog');
  res.render('register', { erro: null });
});

// POST /auth/register
router.post('/register', async (req, res) => {
  const { nome, email, senha } = req.body;

  if (!nome || !email || !senha) {
    return res.render('register', { erro: 'Preencha todos os campos.' });
  }

  if (senha.length < 6) {
    return res.render('register', { erro: 'A senha deve ter pelo menos 6 caracteres.' });
  }

  try {
    const senhaHash = await bcrypt.hash(senha, 10);

    await db.query(
      'INSERT INTO usuarios (nome, email, senha_hash) VALUES (?, ?, ?)',
      [nome, email, senhaHash]
    );

    res.redirect('/auth/login');
  } catch (err) {
    if (err.code === 'ER_DUP_ENTRY') {
      return res.render('register', { erro: 'Este e-mail já está cadastrado.' });
    }
    console.error('Erro no cadastro:', err);
    res.render('register', { erro: 'Erro interno. Tente novamente.' });
  }
});

// GET /auth/logout
router.get('/logout', (req, res) => {
  req.session.destroy(() => {
    res.redirect('/auth/login');
  });
});

module.exports = router;
