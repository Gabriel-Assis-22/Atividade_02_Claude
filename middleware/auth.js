'use strict';

/**
 * Middleware de autenticação.
 * Redireciona para /auth/login se o usuário não tiver sessão ativa.
 */
module.exports = function requireAuth(req, res, next) {
  if (!req.session.userId) {
    return res.redirect('/auth/login');
  }
  next();
};
