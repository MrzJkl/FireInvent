import 'dart:convert';
import 'package:get_it/get_it.dart';

import '../models/clothing_item_assignment_history_model.dart';
import '../services/http_service.dart';

class ClothingAssignmentService {
  final HttpService http = GetIt.I<HttpService>();


  Future<List<ClothingItemAssignmentHistoryModel>> getAll() async {
    final response = await http.get('/assignments');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => ClothingItemAssignmentHistoryModel.fromJson(e)).toList();
  }

  Future<ClothingItemAssignmentHistoryModel> getById(String id) async {
    final response = await http.get('/assignments/$id');
    return ClothingItemAssignmentHistoryModel.fromJson(jsonDecode(response.body));
  }

  Future<ClothingItemAssignmentHistoryModel> create(ClothingItemAssignmentHistoryModel model) async {
    final response = await http.post('/assignments', model.toJson());
    return ClothingItemAssignmentHistoryModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, ClothingItemAssignmentHistoryModel model) async {
    await http.put('/assignments/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/assignments/$id');
  }
}
