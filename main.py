#!/usr/bin/env python

import json
import webapp2
import jinja2
import os

from google.appengine.ext import db

jinja_environment = jinja2.Environment(
    loader=jinja2.FileSystemLoader(os.path.dirname(__file__)))


class Theme(db.Model):
    date = db.DateTimeProperty(auto_now_add=True)
    json = db.StringProperty()


class BaseHandler(webapp2.RequestHandler):
    def dispatch(self):
        self.response.headers.add_header('Access-Control-Allow-Origin', '*')
        self.response.headers.add_header(
            'content-type', 'application/json', charset='utf-8')
        webapp2.RequestHandler.dispatch(self)


class MainHandler(BaseHandler):
    def get(self):
        self.redirect('http://metro-start.com')


class WeatherHandler(BaseHandler):
    def get(self, location, unit):
        r = requests.get(
            'https://api.openweathermap.org/data/2.5/weather?q=%s&units=%s' % location, 'Imperial' if unit == 'f' else 'Metric')
        if r.status_code == 200:
            self.response.write(r.json())


class ThemeJsonHandler(BaseHandler):
    def get(self):
        themes = map(lambda t: json.loads(t.json), db.GqlQuery(
            "SELECT date, json FROM Theme ORDER BY date DESC"))
        self.response.write(json.dumps(themes))


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


# class CloneThemesHandler(BaseHandler):
#     def get(self):
#         oldThemes = db.GqlQuery("SELECT author, title, title_color, background_color, main_color, options_color "
#            "FROM ColorTheme WHERE approved = 1 "
#            "ORDER BY date DESC")

#         for oldTheme in oldThemes:
#             newTheme = Theme()
#             newTheme.json = json.dumps(db.to_dict(oldTheme))
#             newTheme.put()


app = webapp2.WSGIApplication([('/', MainHandler),
                               ('/weather', WeatherHandler),
                               ('/newtheme', NewThemeHandler),
                               ('/themes.json', ThemeJsonHandler)],
                              debug=True)
