import 'dart:convert';
import 'package:fireinvent/models/create_models/create_clothing_product_model.dart';
import 'package:get_it/get_it.dart';

import '../models/clothing_product_model.dart';
import '../services/http_service.dart';

class ClothingProductService {
  final HttpService http = GetIt.I<HttpService>();

  Future<List<ClothingProductModel>> getAll() async {
    final response = await http.get('/clothingProducts');
    final List<dynamic> json = jsonDecode(response.body);
    return json.map((e) => ClothingProductModel.fromJson(e)).toList();
  }

  Future<ClothingProductModel> getById(String id) async {
    final response = await http.get('/clothingProducts/$id');
    return ClothingProductModel.fromJson(jsonDecode(response.body));
  }

  Future<ClothingProductModel> create(CreateClothingProductModel model) async {
    final response = await http.post('/clothingProducts', model.toJson());
    return ClothingProductModel.fromJson(jsonDecode(response.body));
  }

  Future<void> update(String id, ClothingProductModel model) async {
    await http.put('/clothingProducts/$id', model.toJson());
  }

  Future<void> delete(String id) async {
    await http.delete('/clothingProducts/$id');
  }
}
