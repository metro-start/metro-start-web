#!/usr/bin/env python

import json
import webapp2
import jinja2
import os

from google.appengine.ext import db

jinja_environment = jinja2.Environment(
            loader=jinja2.FileSystemLoader(os.path.dirname(__file__)))


class Theme(db.Model):
    author = db.StringProperty()
    title = db.StringProperty()
    author = db.StringProperty()
    email = db.StringProperty()
    website = db.StringProperty()
    title_color = db.StringProperty()
    background_color = db.StringProperty()
    main_color = db.StringProperty()
    options_color = db.StringProperty()
    date = db.DateTimeProperty(auto_now_add=True)
    approved = db.IntegerProperty(default=1)


class MainHandler(webapp2.RequestHandler):
    def get(self):
        template = jinja_environment.get_template('index.html')
        self.response.out.write(template.render())


class ThemesHandler(webapp2.RequestHandler):
    def get(self):
        themes = db.GqlQuery("SELECT author, website, title, title_color, background_color, main_color, options_color "
           "FROM Theme WHERE approved = 1 "
           "ORDER BY date DESC LIMIT 20")

        outThemes = []
        for t in themes:
            outThemes.append(db.to_dict(t))

        template_values = {
            'themes': json.dumps(outThemes),
        }
        template = jinja_environment.get_template('themes.html')
        self.response.out.write(template.render(template_values))


class ThemeJsonHandler(webapp2.RequestHandler):
    def get(self):
        themes = db.GqlQuery("SELECT author, website, title, title_color, background_color, main_color, options_color "
           "FROM Theme WHERE approved = 1 "
           "ORDER BY date DESC")

        self.response.headers.add_header('content-type', 'application/json', charset='utf-8')
        outThemes = []
        for t in themes:
            outThemes.append(db.to_dict(t))

        self.response.write(json.dumps(outThemes))


class NewThemeHandler(webapp2.RequestHandler):
    def get(self):
        err = []
        theme = Theme()
        theme.title = self.request.get('title')
        theme.website = self.request.get('website')
        theme.author = self.request.get('author')
        theme.email = self.request.get('email')
        theme.title_color = self.request.get('titlecolor')
        if theme.title_color.strip() == '':
            err.append('title_color')

        theme.background_color = self.request.get('backgroundcolor')
        if theme.background_color.strip() == '':
            err.append('background_color')

        theme.main_color = self.request.get('maincolor')
        if theme.main_color.strip() == '':
            err.append('main_color')

        theme.options_color = self.request.get('optionscolor')
        if theme.options_color.strip() == '':
            err.append('options_color')

        if len(err) == 0:
            theme.put()
            self.response.out.write(200)
            self.redirect('/themes')
        else:
            template_values = {
                'author': self.request.get('author'),
                'title': self.request.get('title'),
                'author': self.request.get('author'),
                'email': self.request.get('email'),
                'website': self.request.get('website'),
                'title_color': self.request.get('titlecolor'),
                'background_color': self.request.get('backgroundcolor'),
                'main_color': self.request.get('maincolor'),
                'options_color': self.request.get('optionscolor'),
                'err': err,
            }
            template = jinja_environment.get_template('newtheme.html')
            self.response.out.write(template.render(template_values))

app = webapp2.WSGIApplication([('/', MainHandler),
                               ('/themes', ThemesHandler),
                               ('/newtheme', NewThemeHandler),
                               ('/themes.json', ThemeJsonHandler)],
                              debug=True)
