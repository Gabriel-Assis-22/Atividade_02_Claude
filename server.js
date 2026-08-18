'use strict';
require('dotenv').config();

const express = require('express');
const session = require('express-session');
const path = require('path');

const authRoutes = require('./routes/auth');
const catalogRoutes = require('./routes/catalog');
const userRoutes = require('./routes/user');
const requireAuth = require('./middleware/auth');

const app = express();
const PORT = process.env.PORT || 3000;

// View engine
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// Body parsing
app.use(express.urlencoded({ extended: true }));
app.use(express.json());

// Static files
app.use(express.static(path.join(__dirname, 'public')));

// Session
app.use(session({
  secret: process.env.SESSION_SECRET || 'dev_secret_change_in_prod',
  resave: false,
  saveUninitialized: false,
  cookie: { maxAge: 1000 * 60 * 60 * 24 }, // 24h
}));

// Disponibiliza dados do usuário logado para todas as views
app.use((req, res, next) => {
  res.locals.usuario = req.session.usuario || null;
  next();
});

// Rotas
app.use('/auth', authRoutes);
app.use('/', requireAuth, catalogRoutes);
app.use('/user', requireAuth, userRoutes);

// Rota raiz: redireciona para /auth/login se sem sessão (capturada pelo middleware acima)
app.get('/', requireAuth, (req, res) => {
  res.redirect('/catalog');
});

// 404
app.use((req, res) => {
  res.status(404).render('404', { titulo: 'Página não encontrada' });
});

app.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
});
