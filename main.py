#!/usr/bin/env python

import json
import webapp2
import jinja2
import os

from google.appengine.ext import db

jinja_environment = jinja2.Environment(
            loader=jinja2.FileSystemLoader(os.path.dirname(__file__)))


class ColorTheme(db.Model):
    title = db.StringProperty()
    author = db.StringProperty()
    title_color = db.StringProperty()
    background_color = db.StringProperty()
    main_color = db.StringProperty()
    options_color = db.StringProperty()
    date = db.DateTimeProperty(auto_now_add=True)
    approved = db.IntegerProperty(default=1)


class MainHandler(webapp2.RequestHandler):
    def get(self):
        self.redirect('http://metro-start.com')


class ThemeJsonHandler(webapp2.RequestHandler):
    def get(self):
        themes = db.GqlQuery("SELECT author, title, title_color, background_color, main_color, options_color "
           "FROM ColorTheme WHERE approved = 1 "
           "ORDER BY date DESC")

        self.response.headers.add_header('content-type', 'application/json', charset='utf-8')
        outThemes = []
        for t in themes:
            outThemes.append(db.to_dict(t))

        self.response.write(json.dumps(outThemes))


class NewThemeHandler(webapp2.RequestHandler):
    def get(self):
        err = []

        colorTheme = ColorTheme()
        colorTheme.author = self.request.get('author')

        colorTheme.title = self.request.get('title')
        if colorTheme.title.strip() == '':
            err.append('title_color')

        colorTheme.title_color = self.request.get('titlecolor')
        if colorTheme.title_color.strip() == '':
            err.append('title_color')

        colorTheme.background_color = self.request.get('backgroundcolor')
        if colorTheme.background_color.strip() == '':
            err.append('background_color')

        colorTheme.main_color = self.request.get('maincolor')
        if colorTheme.main_color.strip() == '':
            err.append('main_color')

        colorTheme.options_color = self.request.get('optionscolor')
        if colorTheme.options_color.strip() == '':
            err.append('options_color')

        self.response.headers.add_header('content-type', 'application/json', charset='utf-8')
        if len(err) == 0:
            colorTheme.put()
            self.response.out.write(200)
        else:
            self.response.out.write(400)


app = webapp2.WSGIApplication([('/', MainHandler),
                               ('/newtheme', NewThemeHandler),
                               ('/themes.json', ThemeJsonHandler)],
                              debug=True)
