import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter/services.dart';
import 'package:mobile/models/OutlookState.dart';
import 'package:mobile/models/article.dart';
import 'package:mobile/models/category.dart';
import 'package:mobile/models/issue.dart';
import 'package:mobile/models/member.dart';
import 'package:mobile/models/topStats.dart';
import 'package:mobile/models/user.dart';
import 'package:mobile/models/volume.dart';
import 'package:mobile/redux/actions.dart';
import 'package:mobile/services/constants.dart';
import 'package:mobile/services/secrets.dart';
import 'package:redux/redux.dart';

Future<Map<String, dynamic>> getLanguage(String abbreviation) async {
  String languageJsonString = await rootBundle.loadString('assets/languages/$abbreviation.json');
  return json.decode(languageJsonString);
}

Future<List<Volume>> fetchVolumes() async {
  var response = await 
  http.get(_buildUrl(path: 'volumes'));

  if (response.statusCode == 200) {
    Iterable jsonList = json.decode(response.body);
    return jsonList.map((v) => Volume.fromJson(v)).toList();
  } else {
    throw Exception('Failed to load volumes.');
  }
}

Future<List<Issue>> fetchIssues(int volumeId) async {
  var response = await http.get(_buildUrl(path: 'issues/$volumeId'));

  if (response.statusCode == 200) {
    Iterable jsonList = json.decode(response.body);
    return jsonList.map((v) => Issue.fromJson(v)).toList();
  } else {
    throw Exception('Failed to load issues.');
  }
}

Future<List<Category>> fetchCategories(int issueId) async {
  var response = await http.get(_buildUrl(path: 'categories/$issueId'));

  if (response.statusCode == 200) {
    Iterable jsonList = json.decode(response.body);
    return jsonList.map((c) => Category.fromJson(c)).toList();
  } else {
    throw Exception('Failed to load categories.');
  }
}

Future<List<Article>> fetchArticle(int issueId) async {
  var response = await http.get(_buildUrl(path: 'articles/$issueId'));

  if (response.statusCode == 200) {
    Iterable jsonList = json.decode(response.body);
    return jsonList.map((a) => Article.fromJson(a)).toList();
  } else {
    throw Exception('Failed to load articles.');
  }
}

Future<TopStats> fetchTopArticles() async {
  var response = await http.get(_buildUrl(path: 'articles'));

  if (response.statusCode == 200) {
    var topStats = json.decode(response.body);
    List<Article> topVotedArticles = topStats['topRatedArticles'].map<Article>((a) => Article.fromJson(a)).toList();
    List<Article> topFavoritedArticles = topStats['topFavoritedArticles'].map<Article>((a) => Article.fromJson(a)).toList();
    return TopStats(topVotedArticles: topVotedArticles, topFavoritedArticles: topFavoritedArticles);
  } else {
    throw Exception('Failed to load top articles.');
  }
}

Future<TopStats> fetchTopWriters() async {
  var response = await http.get(_buildUrl(path: 'members/top'));

  if (response.statusCode == 200) {
    Iterable topStats = json.decode(response.body);
    List<Member> topWriters = topStats.map<Member>((m) => Member.fromJson(m)).toList();
    return TopStats(topWriters: topWriters);
  } else {
    throw Exception('Failed to load top writers.');
  }
}

Future<List<Member>> fetchWriters() async {
  var response = await http.get(_buildUrl(path: 'members'));

  if (response.statusCode == 200) {
    Iterable jsonList = json.decode(response.body);
    return jsonList.map((w) => Member.fromJson(w)).toList();
  } else {
    throw Exception('Failed to load writers');
  }
}

Future<Member> fetchMember(int memberId) async {
  var response = await http.get(_buildUrl(path: 'members/$memberId'));

  if (response.statusCode == 200) {
    var jsonString = json.decode(response.body);
    return Member.fromJson(jsonString);
  } else {
    throw Exception('Failed to load member');
  }
}

Future<Map<String, dynamic>> signIn(String username, String password) async {
  var response = await http.post(
    'http://$SERVER_URL/connect/token',
    headers: {
      "Content-Type": "application/x-www-form-urlencoded"
    },
    body: {
      "client_id": ClientId,
      "grant_type": "password",
      "username": username,
      "password": password,
      "scope": "outlookApi offline_access openid profile"
    },
  );

  if (response.statusCode == 200) {
    return json.decode(response.body);
  } else {
    return Future.error('Failed to login.');
  }
}

Future<Map<String, dynamic>> signUp(User user) async {
  var response = await http.post(
    'http://$SERVER_URL/api/Identity/register',
    headers: {
      "Content-Type": "application/json"
    },
    body: json.encode(user.toJson())
  );

  if (response.statusCode == 200) {
    return json.decode(response.body);
  } else {
    return Future.error(json.encode(json.decode(response.body)['errors']));
  }
}

void onIssueChange(Store<OutlookState> store, int issueId, SetStateCallback onFinish) {
  fetchCategories(issueId).then((c) {
    store.dispatch(SetCategoriesAction(categories: c));
      // setState(() { });
    onFinish();
  });
  fetchArticle(issueId).then((a) {
    store.dispatch(SetArticlesAction(articles: a));
    onFinish();
  });
  fetchTopWriters().then((t1) {
    fetchTopArticles().then((t2) {
      var topStats = TopStats(topWriters: t1.topWriters, topFavoritedArticles: t2.topFavoritedArticles, topVotedArticles: t2.topVotedArticles);
      store.dispatch(SetTopStatsAction(topStats: topStats));
      onFinish();
    });
  });
  fetchWriters().then((w) {
    store.dispatch(SetWritersAction(writers: w));
    onFinish();
  });
}

void onVolumeChange(Store<OutlookState> store, int volumeId, SetStateCallback onFinish) {
  fetchIssues(volumeId).then((i) {
    store.dispatch(SetIssuesAction(issues: i));
    store.dispatch(SetIssueAction(issue: i.last));
    onIssueChange(store, i.last.id, onFinish);
  });
}

_buildUrl({String path, dynamic params}) =>
  Uri.http(API_URL, path, params);