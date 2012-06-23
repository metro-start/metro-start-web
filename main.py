#!/usr/bin/env python

import cgi
import json
import datetime
import urllib
import urlparse
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

class MainHandler(webapp2.RequestHandler):
	def get(self):
		themes = db.GqlQuery("SELECT * "
		   "FROM Theme "
		   "ORDER BY date DESC LIMIT 10")

		template_values = {
			'themes': themes,
		}
		template = jinja_environment.get_template('index.html')
		self.response.out.write(template.render(template_values))

class NewTheme(webapp2.RequestHandler):
	def get(self):
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
		}
		template = jinja_environment.get_template('newtheme.html')
		self.response.out.write(template.render(template_values))
	
	def post(self):
		theme_name = self.request.get('title')
		theme = Theme()
		theme.author = self.request.get('author')
		theme.title = self.request.get('title')
		theme.author = self.request.get('author')
		theme.email = self.request.get('email')
		theme.website = self.request.get('website')
		url = urlparse.urlparse(theme.website)
		if url.scheme == '':
			newurl = list(url[:])
			newurl[0] = 'http'
			theme.website = urlparse.urlunparse(newurl)

		theme.title_color = self.request.get('title-color')
		theme.background_color = self.request.get('background-color')
		theme.main_color = self.request.get('main-color')
		theme.options_color = self.request.get('options-color')
		theme.put()
		
		self.redirect('/')


class ThemeJson(webapp2.RequestHandler):
	def get(self):
		themes = db.GqlQuery("SELECT author, email, website, title, title_color, background_color, main_color, options_color "
		   "FROM Theme "
		   "ORDER BY date DESC LIMIT 10")
		self.response.headers.add_header('content-type', 'application/json', charset='utf-8')
		resp = [db.to_dict(t) for t in themes]
		self.response.write(json.dumps(resp))
		#self.response.out.write('''[{"title": "summer bliss","author": "chuma","link": "http://www.twitter.com/chustar","colors": {"options-color": "#ff0000","main-color": "#ffff00","title-color": "#4a114a","background-color": "#550000"}},{"title": "midnight run","author": "chuma","link": "http://www.chumannaji.com","colors": {"options-color": "#bf0000","main-color": "#ff8f00","title-color": "#4a114a","background-color": "#050000"}}]''')

app = webapp2.WSGIApplication([('/', MainHandler), 
							   ('/newtheme', NewTheme),
							   ('/themes.json', ThemeJson)],
							   debug=True)
