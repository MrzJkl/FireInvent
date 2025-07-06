import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import '../models/create_models/create_storage_location_model.dart';
import '../models/storage_location_model.dart';

class StorageLocationForm extends StatefulWidget {
  final StorageLocationModel? initial;
  final void Function(CreateStorageLocationModel) onSubmit;

  const StorageLocationForm({super.key, this.initial, required this.onSubmit});

  @override
  State<StorageLocationForm> createState() => _StorageLocationFormState();
}

class _StorageLocationFormState extends State<StorageLocationForm> {
  final _formKey = GlobalKey<FormBuilderState>();

  @override
  Widget build(BuildContext context) {
    return FormBuilder(
      key: _formKey,
      initialValue: {
        'name': widget.initial?.name ?? '',
        'remarks': widget.initial?.remarks ?? '',
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          FormBuilderTextField(
            name: 'name',
            decoration: const InputDecoration(labelText: 'Name'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'remarks',
            decoration: const InputDecoration(labelText: 'Bemerkungen'),
            maxLines: 2,
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Abbrechen'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (_formKey.currentState?.saveAndValidate() ?? false) {
                    final values = _formKey.currentState!.value;
                    widget.onSubmit(
                      CreateStorageLocationModel(
                        name: values['name'] as String,
                        remarks:
                            (values['remarks'] as String).isEmpty
                                ? null
                                : values['remarks'] as String,
                      ),
                    );
                  }
                },
                child: const Text('Speichern'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
