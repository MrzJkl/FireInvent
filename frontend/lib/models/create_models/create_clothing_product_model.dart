import 'package:fireinvent/models/gear_type.dart';
import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_clothing_product_model.g.dart';

@JsonSerializable()
@immutable
class CreateClothingProductModel {
  final String name;
  final String manufacturer;
  final String? description;
  final GearType type;

  const CreateClothingProductModel({
    required this.name,
    required this.manufacturer,
    this.description,
    required this.type,
  });

  factory CreateClothingProductModel.fromJson(Map<String, dynamic> json) =>
      _$CreateClothingProductModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateClothingProductModelToJson(this);
}
