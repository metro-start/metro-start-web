#!/usr/bin/env python

import json
import webapp2
import jinja2
import os

from google.appengine.ext import db

jinja_environment = jinja2.Environment(loader=jinja2.FileSystemLoader(os.path.dirname(__file__)))

class Theme(db.Expando):
    date = db.DateTimeProperty(auto_now_add=True)

class BaseHandler(webapp2.RequestHandler):
    def dispatch(self):
        self.response.headers.add_header('Access-Control-Allow-Origin', '*')
        self.response.headers.add_header('content-type', 'application/json', charset='utf-8')
        webapp2.RequestHandler.dispatch(self)

class MainHandler(BaseHandler):
    def get(self):
        self.redirect('http://metro-start.com')


class ThemeJsonHandler(BaseHandler):
    def get(self):
        themes = map(lambda t: t.json, db.GqlQuery("SELECT json FROM Theme ORDER BY date DESC"))
        self.response.write(themes)


class NewThemeHandler(BaseHandler):
    def post(self):
        err = []
        
        theme = Theme()
        theme.json = next(iter(self.request.arguments()))
        
        if len(err) == 0:
            theme.put()
            self.response.out.write(200)
        else:
            self.response.out.write(400)

    def options(self):
        pass

app = webapp2.WSGIApplication([('/', MainHandler),
                               ('/newtheme', NewThemeHandler),
                               ('/themes.json', ThemeJsonHandler)],
                              debug=True)
