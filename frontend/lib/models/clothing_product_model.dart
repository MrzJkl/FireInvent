import 'package:fireinvent/models/gear_type.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_clothing_product_model.dart';

part 'clothing_product_model.g.dart';

@JsonSerializable()
@immutable
class ClothingProductModel extends CreateClothingProductModel {
  final String id;

  const ClothingProductModel({
    required this.id,
    required super.name,
    required super.manufacturer,
    super.description,
    required super.type,
  });

  factory ClothingProductModel.fromJson(Map<String, dynamic> json) =>
      _$ClothingProductModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$ClothingProductModelToJson(this);
}
