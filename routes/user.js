'use strict';

const express = require('express');
const db = require('../db');

const router = express.Router();

// ─── FAVORITOS ───────────────────────────────────────────────────────────────

// GET /user/favorites — lista os favoritos do usuário logado
router.get('/favorites', async (req, res) => {
  try {
    const [favoritos] = await db.query(
      'SELECT * FROM favoritos WHERE usuario_id = ? ORDER BY criado_em DESC',
      [req.session.userId]
    );
    res.render('favorites', { favoritos });
  } catch (err) {
    console.error('Erro ao buscar favoritos:', err);
    res.render('favorites', { favoritos: [], erro: 'Erro ao carregar favoritos.' });
  }
});

// POST /user/favorites — adiciona um favorito
router.post('/favorites', async (req, res) => {
  const { tmdb_movie_id, titulo, poster_path } = req.body;
  const usuarioId = req.session.userId; // NUNCA vem do body

  if (!tmdb_movie_id || !titulo) {
    return res.redirect('back');
  }

  try {
    await db.query(
      `INSERT IGNORE INTO favoritos (usuario_id, tmdb_movie_id, titulo, poster_path)
       VALUES (?, ?, ?, ?)`,
      [usuarioId, tmdb_movie_id, titulo, poster_path || null]
    );
    res.redirect(`/movie/${tmdb_movie_id}`);
  } catch (err) {
    console.error('Erro ao favoritar:', err);
    res.redirect(`/movie/${tmdb_movie_id}`);
  }
});

// POST /user/favorites/remove — remove um favorito
router.post('/favorites/remove', async (req, res) => {
  const { tmdb_movie_id } = req.body;
  const usuarioId = req.session.userId; // NUNCA vem do body

  try {
    // WHERE duplo: garante isolamento — só remove se for do próprio usuário
    await db.query(
      'DELETE FROM favoritos WHERE tmdb_movie_id = ? AND usuario_id = ?',
      [tmdb_movie_id, usuarioId]
    );
    res.redirect(`/movie/${tmdb_movie_id}`);
  } catch (err) {
    console.error('Erro ao remover favorito:', err);
    res.redirect(`/movie/${tmdb_movie_id}`);
  }
});

// ─── COMENTÁRIOS ─────────────────────────────────────────────────────────────

// GET /user/comments/:movieId — comentários do usuário logado para um filme
router.get('/comments/:movieId', async (req, res) => {
  const movieId = parseInt(req.params.movieId, 10);
  const usuarioId = req.session.userId;

  try {
    const [comentarios] = await db.query(
      `SELECT * FROM comentarios
       WHERE tmdb_movie_id = ? AND usuario_id = ?
       ORDER BY criado_em DESC`,
      [movieId, usuarioId]
    );
    res.json(comentarios);
  } catch (err) {
    console.error('Erro ao buscar comentários:', err);
    res.status(500).json([]);
  }
});

// POST /user/comments — adiciona um comentário
router.post('/comments', async (req, res) => {
  const { tmdb_movie_id, texto } = req.body;
  const usuarioId = req.session.userId; // NUNCA vem do body

  if (!tmdb_movie_id || !texto || !texto.trim()) {
    return res.redirect('back');
  }

  try {
    await db.query(
      'INSERT INTO comentarios (usuario_id, tmdb_movie_id, texto) VALUES (?, ?, ?)',
      [usuarioId, tmdb_movie_id, texto.trim()]
    );
    res.redirect(`/movie/${tmdb_movie_id}`);
  } catch (err) {
    console.error('Erro ao inserir comentário:', err);
    res.redirect(`/movie/${tmdb_movie_id}`);
  }
});

module.exports = router;
