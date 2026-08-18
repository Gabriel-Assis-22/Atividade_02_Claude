'use strict';

const express = require('express');
const axios = require('axios');
const db = require('../db');

const router = express.Router();

const TMDB_BASE = 'https://api.themoviedb.org/3';
const TMDB_IMG  = 'https://image.tmdb.org/t/p/w500';

// Cache do person_id de Tom Hanks (evita chamada extra a cada request)
let tomHanksId = null;

async function getTomHanksId() {
  if (tomHanksId) return tomHanksId;

  const resp = await axios.get(`${TMDB_BASE}/search/person`, {
    params: { query: 'Tom Hanks', language: 'pt-BR' },
    headers: { Authorization: `Bearer ${process.env.TMDB_API_KEY}` },
  });

  tomHanksId = resp.data.results[0].id;
  return tomHanksId;
}

// GET /catalog — lista de filmes de Tom Hanks
router.get('/catalog', async (req, res) => {
  try {
    const personId = await getTomHanksId();

    const resp = await axios.get(`${TMDB_BASE}/person/${personId}/movie_credits`, {
      params: { language: 'pt-BR' },
      headers: { Authorization: `Bearer ${process.env.TMDB_API_KEY}` },
    });

    // Filtra apenas filmes com pôster e título, ordena por popularidade
    const filmes = resp.data.cast
      .filter(f => f.poster_path && f.title)
      .sort((a, b) => b.popularity - a.popularity)
      .map(f => ({
        id: f.id,
        titulo: f.title,
        posterUrl: `${TMDB_IMG}${f.poster_path}`,
        ano: f.release_date ? f.release_date.slice(0, 4) : '—',
      }));

    res.render('catalog', { filmes });
  } catch (err) {
    console.error('Erro ao buscar catálogo TMDB:', err.message);
    res.render('catalog', { filmes: [], erro: 'Não foi possível carregar o catálogo.' });
  }
});

// GET /movie/:id — detalhes de um filme
router.get('/movie/:id', async (req, res) => {
  const movieId = parseInt(req.params.id, 10);
  const usuarioId = req.session.userId;

  if (isNaN(movieId)) return res.redirect('/catalog');

  try {
    // Busca detalhes na TMDB + favorito e comentários do usuário em paralelo
    const [detailResp, [favRows], [comentarios]] = await Promise.all([
      axios.get(`${TMDB_BASE}/movie/${movieId}`, {
        params: { language: 'pt-BR' },
        headers: { Authorization: `Bearer ${process.env.TMDB_API_KEY}` },
      }),
      db.query(
        'SELECT id FROM favoritos WHERE tmdb_movie_id = ? AND usuario_id = ?',
        [movieId, usuarioId]
      ),
      db.query(
        'SELECT * FROM comentarios WHERE tmdb_movie_id = ? AND usuario_id = ? ORDER BY criado_em DESC',
        [movieId, usuarioId]
      ),
    ]);

    const f = detailResp.data;
    const filme = {
      id: f.id,
      titulo: f.title,
      sinopse: f.overview || 'Sinopse não disponível.',
      posterUrl: f.poster_path ? `${TMDB_IMG}${f.poster_path}` : null,
      poster_path: f.poster_path || '',
      ano: f.release_date ? f.release_date.slice(0, 4) : '—',
      nota: f.vote_average ? f.vote_average.toFixed(1) : '—',
    };

    res.render('movie', {
      filme,
      isFavorito: favRows.length > 0,
      comentarios,
    });
  } catch (err) {
    console.error('Erro ao buscar filme TMDB:', err.message);
    res.redirect('/catalog');
  }
});

module.exports = router;
